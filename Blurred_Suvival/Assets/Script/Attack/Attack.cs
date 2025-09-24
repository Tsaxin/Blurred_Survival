using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public int PerformAttack(CharacterStats attacker, CharacterStats target, int _remainingAttacks, GearEquipper gearEquipper = null)
    {
        if (gearEquipper == null || gearEquipper.equippedWeapon == null)
        {
            return ApplyDamage(attacker, target, _remainingAttacks);
        }

        WeaponData weapon = gearEquipper.equippedWeapon;

        switch (weapon.attackPattern)
        {
            case WeaponData.AttackPattern.SingleTarget:
                return ApplyDamage(attacker, target, _remainingAttacks);

            case WeaponData.AttackPattern.SlingshotSplash:
                HandleSlingshotSplash(attacker, target, gearEquipper);
                return ApplyDamage(attacker, target, _remainingAttacks);
            // add more cases for future weapon types
            case WeaponData.AttackPattern atk when atk == WeaponData.AttackPattern.AR || atk == WeaponData.AttackPattern.SMG:
                HandleAR(target, gearEquipper);
                return ApplyDamage(attacker, target, _remainingAttacks);
            case WeaponData.AttackPattern.BoltAction:
                HandleBoltAction(attacker, target, gearEquipper);
                return ApplyDamage(attacker, target, _remainingAttacks);
        }
        return 0;
    }

    #region SlingShot
    private void HandleSlingshotSplash(CharacterStats attacker, CharacterStats target, GearEquipper gearEquipper)
    {
        // Get positions
        Vector2Int playerPos = TileManager.Instance.GetTileIndices(attacker.GetComponent<Tile>().CurrentTileData.transform);
        Vector2Int enemyPos = TileManager.Instance.GetTileIndices(target.GetComponent<Tile>().CurrentTileData.transform);

        WeaponData weaponData = gearEquipper.equippedWeapon;
        int splashDamage = weaponData.SplashDamage;
        float splashAccuracy = weaponData.SplashAccuracy;

        int horizontalDist = Mathf.Abs(playerPos.x - enemyPos.x);

        // CASE 1: Attacker is left/right AND distance > 3
        if (horizontalDist > 3 && playerPos.y == enemyPos.y)
        {
            // Top 3 tiles (enemyPos.y + 1)
            List<Vector2Int> topAdj = new List<Vector2Int>
        {
            new Vector2Int(enemyPos.x - 1, enemyPos.y + 1),
            new Vector2Int(enemyPos.x,     enemyPos.y + 1),
            new Vector2Int(enemyPos.x + 1, enemyPos.y + 1)
        };

            // Bottom 3 tiles (enemyPos.y - 1)
            List<Vector2Int> bottomAdj = new List<Vector2Int>
        {
            new Vector2Int(enemyPos.x - 1, enemyPos.y - 1),
            new Vector2Int(enemyPos.x,     enemyPos.y - 1),
            new Vector2Int(enemyPos.x + 1, enemyPos.y - 1)
        };

            // Pick one occupied tile from top
            Vector2Int? topTarget = PickFirstValid(topAdj);
            if (topTarget.HasValue)
                TrySplashDamage(topTarget.Value.x, topTarget.Value.y, splashDamage, splashAccuracy);

            // Pick one occupied tile from bottom
            Vector2Int? bottomTarget = PickFirstValid(bottomAdj);
            if (bottomTarget.HasValue)
                TrySplashDamage(bottomTarget.Value.x, bottomTarget.Value.y, splashDamage, splashAccuracy);
        }
        else
        {
            // CASE 2: Attacker within <= 3 tiles horizontally
            List<Vector2Int> adjacents = new List<Vector2Int>
        {
            new Vector2Int(enemyPos.x - 1, enemyPos.y), // left
            new Vector2Int(enemyPos.x + 1, enemyPos.y)  // right
        };

            // Find valid occupied adjacents (excluding players)
            List<Vector2Int> validAdj = FilterValid(adjacents);

            if (validAdj.Count >= 2)
            {
                // Both sides occupied → hit both
                foreach (var pos in validAdj)
                    TrySplashDamage(pos.x, pos.y, splashDamage, splashAccuracy);
            }
            else
            {
                // If less than 2 occupied → pick other adjacents randomly
                List<Vector2Int> allAdj = GetAllAdjacent(enemyPos);
                List<Vector2Int> candidates = FilterValid(allAdj);

                // Exclude the attacker and players
                candidates.RemoveAll(pos =>
                {
                    TileData t = TileManager.Instance.GetTileDataAt(pos.x, pos.y);
                    return t != null && t.occupant != null && t.occupant.CompareTag("Player");
                });

                // Pick up to 2 at random
                for (int i = 0; i < Mathf.Min(2, candidates.Count); i++)
                {
                    Vector2Int pick = candidates[Random.Range(0, candidates.Count)];
                    candidates.Remove(pick); // prevent duplicate
                    TrySplashDamage(pick.x, pick.y, splashDamage, splashAccuracy);
                }
            }
        }
    }

    /// <summary> Returns the first occupied, non-player tile from a list. </summary>
    private Vector2Int? PickFirstValid(List<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            TileData t = TileManager.Instance.GetTileDataAt(pos.x, pos.y);
            if (t != null && t.occupant != null)
                return pos;
        }
        return null;
    }

    /// <summary> Filters out invalid or player-occupied tiles. </summary>
    private List<Vector2Int> FilterValid(List<Vector2Int> positions)
    {
        List<Vector2Int> valid = new List<Vector2Int>();
        foreach (var pos in positions)
        {
            TileData t = TileManager.Instance.GetTileDataAt(pos.x, pos.y);
            if (t != null && t.occupant != null && !t.occupant.CompareTag("Player"))
                valid.Add(pos);
        }
        return valid;
    }

    /// <summary> Gets all 8 adjacent tiles. </summary>
    private List<Vector2Int> GetAllAdjacent(Vector2Int center)
    {
        List<Vector2Int> adj = new List<Vector2Int>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                adj.Add(new Vector2Int(center.x + dx, center.y + dy));
            }
        }
        return adj;
    }
    #endregion

    #region AR or SMG
    private void HandleAR(CharacterStats target, GearEquipper gearEquipper)
    {
        // Center position of target
        Vector2Int targetPos = TileManager.Instance.GetTileIndices(
            target.GetComponent<Tile>().CurrentTileData.transform
        );

        int spreadCount = gearEquipper.equippedWeapon.SpreadCount;
        int spreadRadius = gearEquipper.equippedWeapon.BulletSpreadDistance;
        int splashDamage = gearEquipper.equippedWeapon.SplashDamage;
        float splashAccuracy = gearEquipper.equippedWeapon.SplashAccuracy;

        List<Vector2Int> candidateTiles = new List<Vector2Int>();

        // Collect all tiles within spreadRadius
        for (int dx = -spreadRadius; dx <= spreadRadius; dx++)
        {
            for (int dy = -spreadRadius; dy <= spreadRadius; dy++)
            {
                // Skip center (target itself)
                if (dx == 0 && dy == 0) continue;

                Vector2Int pos = new Vector2Int(targetPos.x + dx, targetPos.y + dy);
                TileData tile = TileManager.Instance.GetTileDataAt(pos.x, pos.y);

                if (tile != null && tile.occupant != null && !tile.occupant.CompareTag("Player"))
                {
                    candidateTiles.Add(pos);
                }
            }
        }

        // Shuffle candidates (random order)
        Shuffle(candidateTiles);

        // Apply spread up to SpreadCount
        int hits = 0;
        foreach (var pos in candidateTiles)
        {
            if (hits >= spreadCount) break;

            TrySplashDamage(pos.x, pos.y, splashDamage, splashAccuracy);
            hits++;
        }
    }

    /// <summary> Utility: Fisher–Yates shuffle </summary>
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    #endregion

    #region BoltAction
    private void HandleBoltAction(CharacterStats attacker, CharacterStats target, GearEquipper gearEquipper)
    {
        int penetration = gearEquipper.equippedWeapon.BulletPenetrationDistance;
        int baseDamage = attacker.attack;
        float splashAccuracy = gearEquipper.equippedWeapon.SplashAccuracy; // optional if you want miss chance

        // Get positions
        Vector2Int attackerPos = TileManager.Instance.GetTileIndices(
            attacker.GetComponent<Tile>().CurrentTileData.transform
        );
        Vector2Int targetPos = TileManager.Instance.GetTileIndices(
            target.GetComponent<Tile>().CurrentTileData.transform
        );

        // Compute direction (normalize to step of -1, 0, or 1)
        Vector2Int dir = new Vector2Int(
            Mathf.Clamp(targetPos.x - attackerPos.x, -1, 1),
            Mathf.Clamp(targetPos.y - attackerPos.y, -1, 1)
        );

        // Start at the tile behind the target
        Vector2Int pos = targetPos;

        int currentDamage = baseDamage;

        for (int i = 1; i <= penetration; i++)
        {
            // Each step reduces damage by 50%
            currentDamage = Mathf.Max(1, currentDamage / 2);

            pos += dir; // move one step further along the line

            // Try to damage occupant if valid
            TrySplashDamage(pos.x, pos.y, currentDamage, splashAccuracy);
        }
    }

    #endregion
    private void TrySplashDamage(int col, int row, int damage, float splashAccuracy)
    {
        TileData tile = TileManager.Instance.GetTileDataAt(col, row);
        if (tile != null && tile.occupant != null && !tile.occupant.CompareTag("Player"))
        {
            CharacterStats stats = tile.occupant.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage);
                Debug.Log($"💥 Splash hit {stats.name} at ({col},{row}) for {damage} dmg");
            }
        }
    }

    int ApplyDamage(CharacterStats attacker, CharacterStats target, int _remainingAttacks)
    {
        if (target == null || target.IsDead) return 0;

        int damage = attacker.attack;
        target.TakeDamage(damage, GetComponent<CharacterStats>());

        Debug.Log($"{name} dealt {damage} damage. Remaining: {_remainingAttacks - 1}");

        return _remainingAttacks;
    }
}
