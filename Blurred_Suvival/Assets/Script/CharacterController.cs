using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CharacterController : MonoBehaviour
{
    public TileManager tileManager;
    public TileData currentTileData;
    public SpriteRenderer sr;
    public Color originalColor;
    public TurnManager turnManager;

    private static CharacterController selectedCharacter = null;
    private static bool selectionJustHappened = false;

    private bool isMoving = false;
    public bool hasMoved = false;

    private List<TileData> highlightedTiles = new List<TileData>();

    private GearEquipper weaponEquipper;

    private void Start()
    {
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
            originalColor = sr.color;

        weaponEquipper = GetComponent<GearEquipper>();
        if (weaponEquipper == null)
            Debug.LogWarning("⚠️ No WeaponType found on character.");
    }


    private void OnMouseDown()
    {
        if (UIBlocker.IsPointerOverUI())
            return; // Don't process clicks if the pointer is over UI

        // ⛔ Block input if not player's turn
        if (turnManager == null || !turnManager.IsPlayerTurn())
            return;

        // ⛔ Block input if this character already moved
        if (hasMoved)
            return;

        // ⛔ Block if currently moving
        if (isMoving) return;

        if (selectedCharacter == this)
        {
            Deselect();
            return;
        }

        if (selectedCharacter != null)
            selectedCharacter.Deselect();

        OnSelect();
    }

    void OnSelect()
    {
        selectedCharacter = this;
        selectionJustHappened = true;
        SetButtonStatus(true);
        SetSelectedVisual(true);
        ShowAvailableMoveTiles();
        TurnManager.Instance.SelectedUnit = this;
    }

    private void HandleMeleeAction()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (hit.collider != null)
        {
            GameObject clicked = hit.collider.gameObject;

            // Ignore clicks on self
            if (clicked == gameObject || clicked.transform.IsChildOf(transform)) return;

            if (clicked.CompareTag("Enemy"))
            {
                TryAttackEnemy(clicked);
                return;
            }

            TileData tile = clicked.GetComponent<TileData>();
            if (tile != null)
                TryMoveToTile(tile);
        }
        else
        {
            Deselect();
        }
    }

    private void HandleRangedAction()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (hit.collider != null)
        {
            GameObject clicked = hit.collider.gameObject;

            // Ignore self
            if (clicked == gameObject || clicked.transform.IsChildOf(transform)) return;

            // 📌 Check if clicked on an enemy
            if (clicked.CompareTag("Enemy"))
            {
                CharacterStats enemyStats = clicked.GetComponent<CharacterStats>();
                if (enemyStats != null)
                {
                    CharacterStats myStats = GetComponent<CharacterStats>();
                    ZombieAIBase enemyAI = clicked.GetComponent<ZombieAIBase>();

                    if (myStats != null && enemyAI != null && currentTileData != null && enemyAI.currentTileData != null)
                    {
                        Vector2Int myTilePos = GetTileIndices(currentTileData.transform);
                        Vector2Int enemyTilePos = GetTileIndices(enemyAI.currentTileData.transform);

                        int tileDistance = Mathf.Max(Mathf.Abs(myTilePos.x - enemyTilePos.x), Mathf.Abs(myTilePos.y - enemyTilePos.y)); // Chebyshev distance

                        float rangeBoost = weaponEquipper?.equippedWeapon?.rangeBoost ?? 0;
                        int effectiveRange = myStats.MovementRange + Mathf.RoundToInt(rangeBoost);

                        if (tileDistance <= effectiveRange)
                        {
                            Debug.Log("🏹 Enemy in **ranged** range! Performing ranged attack.");
                            PerformAttack(clicked);
                            Deselect();
                            return;
                        }
                        else
                        {
                            Debug.Log("❌ Enemy is out of range.");
                        }

                    }

                }
            }

            // 📦 Check if clicked on a tile (move to it)
            TileData tile = clicked.GetComponent<TileData>();
            if (tile != null)
            {
                TryMoveToTile(tile);
            }
        }
        else
        {
            Deselect();
        }
    }


    private void Update()
    {
        if (selectedCharacter != this || isMoving)
            return;

        if (selectionJustHappened)
        {
            selectionJustHappened = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // ⛔ Prevent deselecting when clicking on UI (like floating skill buttons)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (weaponEquipper == null) return;

            if (weaponEquipper.equippedWeapon == null)
            {
                HandleMeleeAction();
                return;
            }

            switch (weaponEquipper.equippedWeapon.type)
            {
                case WeaponData.Type.Melee:
                    HandleMeleeAction();
                    break;

                case WeaponData.Type.Range:
                    HandleRangedAction();
                    break;

                default:
                    Deselect();
                    break;
            }
        }


    }

    public void Deselect()
    {
        if (selectedCharacter == this)
        {
            SetSelectedVisual(false);
            ClearHighlightedTiles();
            selectedCharacter = null;
            SetButtonStatus(false);
            TurnManager.Instance.SelectedUnit = null;
        }
    }

    private void SetSelectedVisual(bool selected)
    {
        if (sr != null)
            sr.color = selected ? Color.white : originalColor;
    }

    public void ShowAvailableMoveTiles()
    {
        ClearHighlightedTiles();
        if (tileManager == null || currentTileData == null) return;

        Vector2Int currentPos = GetTileIndices(currentTileData.transform);
        int moveRange = GetComponent<CharacterStats>()?.MovementRange ?? 1;

        for (int dy = -moveRange; dy <= moveRange; dy++)
        {
            for (int dx = -moveRange; dx <= moveRange; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) > moveRange) continue;

                int newX = currentPos.x + dx;
                int newY = currentPos.y + dy;

                if (!IsInBounds(newX, newY)) continue;

                TileData tile = tileManager.tiles[newY, newX].GetComponent<TileData>();
                if (tile == null) continue;

                if (!tile.IsOccupied || tile.occupant.CompareTag("Enemy"))
                {
                    tile.ShowAsPossibleMove(); // normal move tile
                    highlightedTiles.Add(tile);
                }
            }
        }

        // 👇 Add this block
        if (weaponEquipper?.equippedWeapon != null && weaponEquipper.equippedWeapon.type == WeaponData.Type.Range)
        {
            ShowRangedAttackTiles(currentPos, moveRange, weaponEquipper.equippedWeapon.rangeBoost);
        }
    }

    void ShowRangedAttackTiles(Vector2Int origin, int moveRange, float rangeBoost)
    {
        int attackRange = moveRange + Mathf.RoundToInt(rangeBoost);

        for (int dy = -attackRange; dy <= attackRange; dy++)
        {
            for (int dx = -attackRange; dx <= attackRange; dx++)
            {
                if (dx == 0 && dy == 0) continue;

                int dist = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
                if (dist <= moveRange || dist > attackRange)
                    continue; // Skip inner tiles (movement area) and outer too far

                int x = origin.x + dx;
                int y = origin.y + dy;

                if (!IsInBounds(x, y)) continue;

                TileData tile = tileManager.tiles[y, x].GetComponent<TileData>();
                if (tile != null)
                {
                    tile.ShowRangeColor(); // ✨ Show special range color
                    highlightedTiles.Add(tile); // so it gets cleared later
                }
            }
        }
    }

    bool IsInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 &&
               y < tileManager.tiles.GetLength(0) &&
               x < tileManager.tiles.GetLength(1);
    }

    void ClearHighlightedTiles()
    {
        foreach (TileData tile in highlightedTiles)
        {
            tile.ResetColor();
        }
        highlightedTiles.Clear();
    }

    void TryMoveToTile(TileData targetTile)
    {
        SetButtonStatus(false);

        if (hasMoved || isMoving || tileManager == null || currentTileData == null)
            return;

        if (targetTile.IsOccupied)
        {
            Debug.Log("❌ Tile occupied.");
            return;
        }

        Vector2Int currentPos = GetTileIndices(currentTileData.transform);
        Vector2Int targetPos = GetTileIndices(targetTile.transform);

        if (targetPos == new Vector2Int(-1, -1))
        {
            Debug.LogError("❌ Invalid target tile.");
            return;
        }

        int range = GetComponent<CharacterStats>()?.MovementRange ?? 1;
        int dx = Mathf.Abs(currentPos.x - targetPos.x);
        int dy = Mathf.Abs(currentPos.y - targetPos.y);

        if (Mathf.Max(dx, dy) > range)
        {
            Debug.Log("❌ Tile out of range.");
            return;
        }


        StartCoroutine(MoveToTile(targetTile, targetPos));
    }

    void SetScale(Transform targetTransform)
    {
        if (currentTileData == null) return;

        int ResultScale = TileManager.Instance.GetXDirection(currentTileData.transform, targetTransform);
        transform.localScale = new Vector3(ResultScale, transform.localScale.y, transform.localScale.z);

        // also fix UI child scaling (so it doesn’t flip)
        Canvas childCanvas = GetComponentInChildren<Canvas>();
        if (childCanvas != null)
        {
            childCanvas.transform.localScale = new Vector3(
                ResultScale * Mathf.Abs(childCanvas.transform.localScale.x),
                childCanvas.transform.localScale.y,
                childCanvas.transform.localScale.z
            );
        }
    }

    public IEnumerator MoveToTile(TileData targetTile, Vector2Int targetPos, bool skipTurnEnd = false)
    {
        Deselect();
        SetScale(targetTile.transform);

        isMoving = true;
        GetComponent<Animator>().SetBool("IsMoving", isMoving);

        currentTileData.ClearOccupant();

        Vector3 start = transform.position;
        Vector3 end = targetTile.transform.position;

        int spacing = 5; // gap between layers (10, 15, 20, etc.)
        int sortingOrder = 10 + GetTileIndices(targetTile.transform).y * spacing;

        end.z = -sortingOrder * 0.01f;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = sortingOrder;
        SetWeaponSL(sortingOrder);

        float distance = Vector3.Distance(start, end);
        float moveSpeed = GetComponent<CharacterStats>()?.moveSpeed ?? 1f;
        float duration = distance / Mathf.Max(moveSpeed, 0.01f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // smoothstep
            transform.position = Vector3.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        targetTile.AssignOccupant(gameObject);
        currentTileData = targetTile;

        isMoving = false;
        GetComponent<Animator>().SetBool("IsMoving", isMoving);
        hasMoved = true;

        TileData tile = targetTile.GetComponent<TileData>();
        if (tile.HasLoot)
        {
            List<GameObject> lootItems = tile.CollectAllLoot();
            foreach (var loot in lootItems)
            {
                ItemPickUp pickup = loot.GetComponent<ItemPickUp>();
                if (pickup != null)
                {
                    PlayerInventory.Instance.AddItem(pickup.itemData);
                    Destroy(loot);
                }
            }
        }


        Deselect();

        if (!skipTurnEnd)
            turnManager?.OnPlayerFinishedMove(this);
    }


    void TryAttackEnemy(GameObject enemyObject)
    {
        if (hasMoved || isMoving) return;

        if (!enemyObject.TryGetComponent(out ZombieAIBase enemyAI)) return;

        if (currentTileData == null || enemyAI.currentTileData == null)
            return;

        Vector2Int myPos = GetTileIndices(currentTileData.transform);
        Vector2Int enemyPos = GetTileIndices(enemyAI.currentTileData.transform);

        int moveRange = GetComponent<CharacterStats>()?.MovementRange ?? 1;

        // Calculate Chebyshev distance between player and enemy tiles
        int distToEnemy = Mathf.Max(Mathf.Abs(myPos.x - enemyPos.x), Mathf.Abs(myPos.y - enemyPos.y));

        // Only allow move+attack if enemy is within movement range (including adjacent)
        if (distToEnemy > moveRange)
        {
            Debug.Log("❌ Enemy is out of movement range. Cannot move + attack.");
            return;
        }

        // If enemy is already adjacent (attack range 1), attack immediately
        int dx = Mathf.Abs(myPos.x - enemyPos.x);
        int dy = Mathf.Abs(myPos.y - enemyPos.y);
        if ((dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0))
        {
            PerformAttack(enemyObject);
            return;
        }

        // Enemy is within movement range but not adjacent — find a tile adjacent to enemy we can move to within move range
        List<TileData> adjacentTiles = GetAdjacentTiles(enemyPos);

        TileData bestTile = null;
        float bestDistance = float.MaxValue;

        foreach (TileData tile in adjacentTiles)
        {
            if (!tile.IsOccupied)
            {
                Vector2Int tilePos = GetTileIndices(tile.transform);
                int distToPlayerTile = Mathf.Max(Mathf.Abs(myPos.x - tilePos.x), Mathf.Abs(myPos.y - tilePos.y));
                if (distToPlayerTile <= moveRange)
                {
                    float d = Vector2.Distance(transform.position, tile.transform.position);
                    if (d < bestDistance)
                    {
                        bestTile = tile;
                        bestDistance = d;
                    }
                }
            }
        }

        if (bestTile != null)
        {
            // Move to the adjacent tile and attack
            StartCoroutine(MoveAndAttack(bestTile, enemyObject));
        }
        else
        {
            Debug.Log("❌ Cannot reach a tile adjacent to enemy within movement range.");
        }
    }


    IEnumerator MoveAndAttack(TileData targetTile, GameObject enemy)
    {
        yield return MoveToTile(targetTile, GetTileIndices(targetTile.transform), skipTurnEnd: true);
        PerformAttack(enemy); // ends turn after attack
    }


    void PerformAttack(GameObject enemyObject)
    {
        if (enemyObject.TryGetComponent(out CharacterStats enemyStats))
        {
            // 🧭 Face the enemy before attacking
            if (enemyObject.TryGetComponent(out ZombieAIBase enemyAI) && enemyAI.currentTileData != null)
            {
                SetScale(enemyAI.currentTileData.transform);
            }
            StartAttack(enemyStats);
            //StartCoroutine(PerformMultiAttack(enemyStats));
        }
    }

    IEnumerator PerformMultiAttack(CharacterStats targetStats)
    {
        CharacterStats myStats = GetComponent<CharacterStats>();

        for (int i = 0; i < myStats.AttackCount; i++)
        {
            if (targetStats == null || targetStats.IsDead)
            {
                Debug.Log("☠️ Target is dead. Stopping further attacks.");
                break;
            }

            int damage = myStats.attack;
            targetStats.TakeDamage(damage, myStats);
            Debug.Log($"{name} attacked {targetStats.name} ({i + 1}/{myStats.AttackCount}) for {damage} damage.");

            yield return new WaitForSeconds(0.2f); // optional delay between attacks
        }
        FinishedTurn();
    }

    #region Actual attack Region
    private CharacterStats _targetStats;
    private int _remainingAttacks;

    public void StartAttack(CharacterStats target)
    {
        _targetStats = target;

        _remainingAttacks = GetComponent<CharacterStats>().AttackCount; // e.g., 2 attacks
        PlayAttackAnimation();
    }

    private void PlayAttackAnimation()
    {
        if (_targetStats == null || _targetStats.IsDead)
        {
            Debug.Log("❌ No valid target to attack.");
            FinishedTurn();
            return;
        }

        Animator anim = GetComponent<Animator>();
        if (GetComponent<GearEquipper>().equippedWeapon.type==WeaponData.Type.Range) {
            anim.SetTrigger("Range");
        }
        else {
            anim.SetTrigger("Melee");
        }
    }

    // Called by animation event when swing happens
    public void DealAttackDamage()
    {
        if (_targetStats == null || _targetStats.IsDead) return;

        int damage = GetComponent<CharacterStats>().attack;
        _targetStats.TakeDamage(damage, GetComponent<CharacterStats>());

        Debug.Log($"{name} dealt {damage} damage. Remaining: {_remainingAttacks - 1}");
    }

    // Called by animation event at the END of the animation
    public void CheckNextAttack()
    {
        _remainingAttacks--;

        if (_remainingAttacks > 0 && _targetStats != null && !_targetStats.IsDead)
        {
            Debug.Log($"{name} is chaining another attack. {_remainingAttacks} left.");
            PlayAttackAnimation(); // replay animation
        }
        else
        {
            Debug.Log($"{name} finished all attacks.");
            FinishedTurn();
        }
    }

    #endregion

    public void FinishedTurn()
    {
        hasMoved = true;
        Deselect();
        turnManager?.OnPlayerFinishedMove(this);
    }


    List<TileData> GetAdjacentTiles(Vector2Int pos)
    {
        List<TileData> result = new List<TileData>();
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;

                int newX = pos.x + dx;
                int newY = pos.y + dy;

                if (newX < 0 || newY < 0 || newX >= tileManager.tiles.GetLength(1) || newY >= tileManager.tiles.GetLength(0))
                    continue;

                TileData tile = tileManager.tiles[newY, newX].GetComponent<TileData>();
                if (tile != null)
                    result.Add(tile);
            }
        }
        return result;
    }
    public Vector2Int GetTileIndices(Transform tile)
    {
        for (int row = 0; row < tileManager.tiles.GetLength(0); row++)
        {
            for (int col = 0; col < tileManager.tiles.GetLength(1); col++)
            {
                if (tileManager.tiles[row, col] == tile)
                    return new Vector2Int(col, row);
            }
        }
        return new Vector2Int(-1, -1);
    }

    public void BeginTurn()
    {
        hasMoved = false;
    }

    #region Show Buttons
    public GameObject ButtonHolder;
    void SetButtonStatus(bool value)
    {
        ButtonHolder.SetActive(value);
    }
    #endregion

    #region WeaponSprite
    public SpriteRenderer Melee, Range,RangeFlash;
    public TrailRenderer TR;
    public void SetWeaponSL(int sortingOrder)
    {
        Melee.sortingOrder = sortingOrder + 2;
        TR.sortingOrder = sortingOrder + 1;
        Range.sortingOrder = sortingOrder + 2;
        RangeFlash.sortingOrder=sortingOrder + 2;
    }

    public void SetRangeWeapon(Sprite sprite)
    {
        Melee.gameObject.SetActive(false);
        Range.gameObject.SetActive(true);
        Range.sprite = sprite;
    }
    public void SetMeleeWeapon(Sprite sprite)
    {
        Melee.enabled = true;
        Melee.gameObject.SetActive(true);
        Range.gameObject.SetActive(false);
        Melee.sprite = sprite;
    }
    public void SetFist()
    {
        Melee.enabled = false;
        Melee.gameObject.SetActive(true);
        Range.gameObject.SetActive(false);
    }
    #endregion
}
