using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CharacterController : MonoBehaviour
{
    public SpriteRenderer sr;
    public Color originalColor;
    public TurnManager turnManager;

    private static CharacterController selectedCharacter = null;
    private static bool selectionJustHappened = false;

    private bool isMoving = false;
    public bool hasMoved = false;
    private GearEquipper weaponEquipper;

    private void Start()
    {
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
            originalColor = sr.color;

        weaponEquipper = GetComponent<GearEquipper>();
        //if (weaponEquipper == null)
        //Debug.LogWarning("⚠️ No WeaponType found on character.");
    }

    void OnDisable()
    {
        ResetEverythingOnRetreat();
    }

    public void OnClicked(bool Manual = true)
    {
        if (UIBlocker.IsPointerOverUI() && Manual)
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

        if (!this.enabled) return;

        if (selectedCharacter != null)
            selectedCharacter.Deselect();

        OnSelect();
    }

    void OnSelect()
    {
        GetComponentInChildren<Canvas>().sortingOrder++;
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

            HandleTileClicked(clicked);
        }
        else
        {
            Deselect();
        }
    }

    void HandleTileClicked(GameObject clicked)
    {
        TileData tile = clicked.GetComponent<TileData>();
        if (tile != null)
        {
            // Check if tile has a character on it
            if (tile.IsOccupied)
            {
                CharacterController character = tile.occupant.GetComponent<CharacterController>();
                if (character != null && character.CompareTag("Player"))
                {
                    // Select this character if it's a player
                    character.OnSelect();
                    return;
                }
            }

            // Otherwise, try moving current selected character
            TryMoveToTile(tile);
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
                clicked = GetEnemyObject(clicked);
                CharacterStats enemyStats = clicked.GetComponent<CharacterStats>();
                if (enemyStats != null)
                {
                    CharacterStats myStats = GetComponent<CharacterStats>();
                    ZombieAIBase enemyAI = clicked.GetComponent<ZombieAIBase>();

                    if (myStats != null && enemyAI != null && GetComponent<Tile>().CurrentTileData != null && enemyAI.GetComponent<Tile>().CurrentTileData != null)
                    {
                        Vector2Int myTilePos = GetTileIndices(GetComponent<Tile>().CurrentTileData.transform);
                        Vector2Int enemyTilePos = GetTileIndices(enemyAI.GetComponent<Tile>().CurrentTileData.transform);

                        int tileDistance = Mathf.Max(Mathf.Abs(myTilePos.x - enemyTilePos.x), Mathf.Abs(myTilePos.y - enemyTilePos.y)); // Chebyshev distance

                        float rangeBoost = weaponEquipper?.equippedWeapon?.rangeBoost ?? 0;
                        int effectiveRange = myStats.MovementRange + Mathf.RoundToInt(rangeBoost);

                        if (tileDistance <= effectiveRange)
                        {
                            PerformAttack(clicked);
                            Deselect();
                            return;
                        }
                        else
                        {
                            //Debug.Log("❌ Enemy is out of range.");
                        }

                    }

                }
            }

            HandleTileClicked(clicked);
        }
        else
        {
            Deselect();
        }
    }


    private void Update()
    {
        if (selectedCharacter != this || isMoving || isAttacking)
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
            GetComponentInChildren<Canvas>().sortingOrder--;
            SetSelectedVisual(false);
            TileManager.Instance.ClearHighlightedTiles();
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
        TileManager.Instance.ClearHighlightedTiles(); // 👈 clear globally

        if (TileManager.Instance == null || GetComponent<Tile>().CurrentTileData == null)
            return;

        // ✅ Highlight the tile underneath the current character in GREEN
        TileData currentTile = GetComponent<Tile>().CurrentTileData;
        if (currentTile != null)
        {
            TileManager.Instance.HighlightTile(currentTile, isRange: false, isSelectedTile: true);
        }

        Vector2Int currentPos = GetTileIndices(currentTile.transform);
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

                TileData tile = TileManager.Instance.tiles[newY, newX].GetComponent<TileData>();
                if (tile == null) continue;

                if (!tile.IsOccupied || (tile.occupant != null && tile.occupant.CompareTag("Enemy")))
                {
                    TileManager.Instance.HighlightTile(tile);
                }
            }
        }

        if (weaponEquipper?.equippedWeapon != null &&
            weaponEquipper.equippedWeapon.type == WeaponData.Type.Range)
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
                if (dist <= moveRange || dist > attackRange) continue;

                int x = origin.x + dx;
                int y = origin.y + dy;

                if (!IsInBounds(x, y)) continue;

                TileData tile = TileManager.Instance.tiles[y, x].GetComponent<TileData>();
                if (tile != null)
                    TileManager.Instance.HighlightTile(tile, isRange: true);
            }
        }
    }


    bool IsInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 &&
               y < TileManager.Instance.tiles.GetLength(0) &&
               x < TileManager.Instance.tiles.GetLength(1);
    }

    void TryMoveToTile(TileData targetTile)
    {
        if (hasMoved || isMoving || TileManager.Instance == null || GetComponent<Tile>().CurrentTileData == null)
            return;

        if (targetTile.IsOccupied)
        {
            //Debug.Log("❌ Tile occupied.");
            return;
        }

        Vector2Int currentPos = GetTileIndices(GetComponent<Tile>().CurrentTileData.transform);
        Vector2Int targetPos = GetTileIndices(targetTile.transform);

        if (targetPos == new Vector2Int(-1, -1))
        {
            //Debug.LogError("❌ Invalid target tile.");
            return;
        }

        int range = GetComponent<CharacterStats>()?.MovementRange ?? 1;
        int dx = Mathf.Abs(currentPos.x - targetPos.x);
        int dy = Mathf.Abs(currentPos.y - targetPos.y);

        if (Mathf.Max(dx, dy) > range)
        {
            //Debug.Log("❌ Tile out of range.");
            return;
        }

        SetButtonStatus(false);
        StartCoroutine(MoveToTile(targetTile, targetPos));
    }

    public IEnumerator MoveToTile(TileData targetTile, Vector2Int targetPos, bool skipTurnEnd = false)
    {
        Deselect();
        SetScale(targetTile.transform);

        isMoving = true;
        GetComponent<Animator>().SetBool("IsMoving", isMoving);

        GetComponent<Tile>().CurrentTileData.ClearOccupant();

        Vector3 start = transform.position;
        Vector3 end = targetTile.transform.position;

        int spacing = 5; // gap between layers (10, 15, 20, etc.)
        int sortingOrder = 10 + GetTileIndices(targetTile.transform).y * spacing;

        end.z = -sortingOrder * 0.01f;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = sortingOrder;
        GetComponent<GearEquipper>()?.SetWeaponSL(sortingOrder);

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
        GetComponent<Tile>().CurrentTileData = targetTile;

        isMoving = false;
        GetComponent<Animator>().SetBool("IsMoving", isMoving);
        hasMoved = true;

        TileData tile = targetTile.GetComponent<TileData>();
        if (tile.HasLoot)
        {
            tile.TryCollectLoot();
        }
        Deselect();

        if (!skipTurnEnd)
            FinishedTurn();
    }


    void TryAttackEnemy(GameObject enemyObject)
    {
        if (hasMoved || isMoving) return;

        ZombieAIBase enemyAI = GetEnemyObject(enemyObject).GetComponent<ZombieAIBase>();

        if (GetComponent<Tile>().CurrentTileData == null || enemyAI.GetComponent<Tile>().CurrentTileData == null)
            return;

        Vector2Int myPos = GetTileIndices(GetComponent<Tile>().CurrentTileData.transform);
        Vector2Int enemyPos = GetTileIndices(enemyAI.GetComponent<Tile>().CurrentTileData.transform);

        int moveRange = GetComponent<CharacterStats>()?.MovementRange ?? 1;

        // Calculate Chebyshev distance between player and enemy tiles
        int distToEnemy = Mathf.Max(Mathf.Abs(myPos.x - enemyPos.x), Mathf.Abs(myPos.y - enemyPos.y));

        // Only allow move+attack if enemy is within movement range (including adjacent)
        if (distToEnemy > moveRange)
        {
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
            //Debug.Log("❌ Cannot reach a tile adjacent to enemy within movement range.");
        }
    }

    GameObject GetEnemyObject(GameObject Enemy)
    {
        if (Enemy.GetComponent<ZombieAIBase>() != null)
        {
            return Enemy;
        }
        else if (Enemy.transform.parent.GetComponent<ZombieAIBase>() != null)
            return Enemy.transform.parent.gameObject;

        return null;
    }

    IEnumerator MoveAndAttack(TileData targetTile, GameObject enemy)
    {
        yield return MoveToTile(targetTile, GetTileIndices(targetTile.transform), skipTurnEnd: true);
        PerformAttack(enemy); // ends turn after attack
    }


    void PerformAttack(GameObject enemyObject)
    {
        enemyObject = GetEnemyObject(enemyObject);
        if (enemyObject.TryGetComponent(out CharacterStats enemyStats))
        {
            // 🧭 Face the enemy before attacking
            if (enemyObject.TryGetComponent(out ZombieAIBase enemyAI) && enemyAI.GetComponent<Tile>().CurrentTileData != null)
            {
                SetScale(enemyAI.GetComponent<Tile>().CurrentTileData.transform);
            }
            StartAttack(enemyStats);
        }
    }

    #region Actual attack Region
    private CharacterStats _targetStats;
    private int _remainingAttacks;
    private bool isAttacking = false;

    public void StartAttack(CharacterStats target)
    {
        if (isAttacking) return; // BLOCK new attack
        isAttacking = true;
        _targetStats = target;

        _remainingAttacks = GetComponent<CharacterStats>().AttackCount; // e.g., 2 attacks
        PlayAttackAnimation();
    }

    private void PlayAttackAnimation()
    {
        if (_targetStats == null || _targetStats.IsDead)
        {
            //Debug.Log("❌ No valid target to attack.");
            FinishedTurn();
            return;
        }

        Animator anim = GetComponent<Animator>();
        if (GetComponent<GearEquipper>().equippedWeapon != null)
        {
            anim.speed = GetComponent<GearEquipper>().equippedWeapon.AttackSpeed;
            if (GetComponent<GearEquipper>().equippedWeapon.type == WeaponData.Type.Range)
            {
                anim.SetTrigger("Range");
            }
            else
            {
                anim.SetTrigger("Melee");
            }
        }
        else
        {
            anim.SetTrigger("Melee");
        }
    }

    // Called by animation event when swing happens
    public void DealAttackDamage()
    {
        _remainingAttacks = GetComponent<Attack>().PerformAttack(GetComponent<CharacterStats>(), _targetStats, _remainingAttacks, GetComponent<GearEquipper>());
    }

    // Called by animation event at the END of the animation
    public void CheckNextAttack()
    {
        _remainingAttacks--;

        if (_remainingAttacks > 0 && _targetStats != null && !_targetStats.IsDead)
        {
            PlayAttackAnimation(); // replay animation
        }
        else
        {
            isAttacking = false;
            FinishedTurn();
        }
    }

    #endregion

    #region Scale
    void SetScale(Transform targetTransform)
    {
        if (GetComponent<Tile>().CurrentTileData == null) return;

        int ResultScale = TileManager.Instance.GetXDirection(GetComponent<Tile>().CurrentTileData.transform, targetTransform);
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

    public void SetScale(float scaleX)
    {
        // Apply scale to this object
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);

        // Fix UI child scaling so it doesn’t flip
        Canvas childCanvas = GetComponentInChildren<Canvas>();
        if (childCanvas != null)
        {
            childCanvas.transform.localScale = new Vector3(
                scaleX * Mathf.Abs(childCanvas.transform.localScale.x),
                childCanvas.transform.localScale.y,
                childCanvas.transform.localScale.z
            );
        }
    }

    public void ResetScale()
    {
        transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        // also fix UI child scaling (so it doesn’t flip)
        Canvas childCanvas = GetComponentInChildren<Canvas>();
        if (childCanvas != null)
        {
            childCanvas.transform.localScale = new Vector3(
                Mathf.Abs(childCanvas.transform.localScale.x),
                childCanvas.transform.localScale.y,
                childCanvas.transform.localScale.z
            );
        }
    }
    #endregion

    private bool turnEnded = false;

    public void FinishedTurn()
    {
        if (turnEnded) return; // prevent double calls
        GetComponent<Animator>().speed = 1f;
        turnEnded = true;

        hasMoved = true;
        Deselect();
        GetComponent<TurnIndicator>().SetIndicator(false);
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

                if (newX < 0 || newY < 0 || newX >= TileManager.Instance.tiles.GetLength(1) || newY >= TileManager.Instance.tiles.GetLength(0))
                    continue;

                TileData tile = TileManager.Instance.tiles[newY, newX].GetComponent<TileData>();
                if (tile != null)
                    result.Add(tile);
            }
        }
        return result;
    }
    public Vector2Int GetTileIndices(Transform tile)
    {
        for (int row = 0; row < TileManager.Instance.tiles.GetLength(0); row++)
        {
            for (int col = 0; col < TileManager.Instance.tiles.GetLength(1); col++)
            {
                if (TileManager.Instance.tiles[row, col] == tile)
                    return new Vector2Int(col, row);
            }
        }
        return new Vector2Int(-1, -1);
    }

    public void BeginTurn()
    {
        hasMoved = false;
        turnEnded = false; // reset flag
        GetComponent<TurnIndicator>().SetIndicator(true);
    }

    #region Show Buttons
    public GameObject ButtonHolder;
    void SetButtonStatus(bool value)
    {
        ButtonHolder.GetComponent<Animator>().SetBool("Show", value);
        if (value)
        {
            GetComponent<CharacterButtons>().LoadTriggerText();
        }
    }
    #endregion

    #region Retreat
    public void OnRetreat()
    {
        Deselect();
        SetButtonStatus(false);
        // Force character to face left when retreating
        SetScale(-1);

        GetComponent<Animator>().SetBool("IsMoving", true);
        StartCoroutine(TryAndRetreat());
    }

    IEnumerator TryAndRetreat()
    {
        yield return new WaitForSeconds(0.5f);

        if (Squad.Instance.RetreatSuccess)
        {
            Squad.Instance.LoadRetreatAnimationForAll(this.gameObject);

            Vector3 start = transform.position;
            Vector3 end = TileManager.Instance.LeftRetreatTile.transform.position;
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
            SquadMover.Instance.ExitEncounter();
        }
        else
        {
            // Retreat failed → face back right and idle
            SetScale(1);
            GetComponent<Animator>().SetBool("IsMoving", false);
            FinishedTurn();
        }
    }

    void ResetEverythingOnRetreat()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
        GetComponent<Animator>().SetBool("IsMoving", false);
        Animator anim = GetComponent<Animator>();
        anim.Rebind();
        anim.Update(0f);
    }

    public IEnumerator OnRetreatAll(bool isLeft)
    {
        if (this == null) yield break; // safety check at start

        SetScale(-1);

        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetBool("IsMoving", true);

        Vector3 start = transform.position;
        Vector3 end;

        if (isLeft)
        {
            end = TileManager.Instance.LeftRetreatTile.transform.position;
        }
        else
        {
            SetScale(1);
            end = TileManager.Instance.RightRetreatTile.transform.position;
        }

        float distance = Vector3.Distance(start, end);

        // 👉 If the target is very close, snap and exit immediately
        if (distance < 0.1f)
        {
            if (this != null && transform != null)
                transform.position = end;

            if (anim != null) anim.SetBool("IsMoving", false);
            yield break;
        }

        float moveSpeed = GetComponent<CharacterStats>()?.moveSpeed ?? 1f;
        float duration = distance / Mathf.Max(moveSpeed, 0.01f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (this == null || transform == null) yield break;

            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // smoothstep
            transform.position = Vector3.Lerp(start, end, t);

            elapsed += Time.deltaTime;

            // 👉 Also break out if we get close enough mid-way
            if (Vector3.Distance(transform.position, end) < 0.1f)
                break;

            yield return null;
        }

        if (this != null && transform != null)
            transform.position = end;

        if (anim != null) anim.SetBool("IsMoving", false);
    }

    #endregion

}
