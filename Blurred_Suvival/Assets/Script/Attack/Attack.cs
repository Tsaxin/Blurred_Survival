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
                HandleSlingshotSplash(attacker, target,gearEquipper);
                return ApplyDamage(attacker, target, _remainingAttacks);
                // add more cases for future weapon types
        }
        return 0;
    }

    private void HandleSlingshotSplash(CharacterStats attacker, CharacterStats target,GearEquipper gearEquipper)
    {
        // Get positions (col = x, row = y)
        Vector2Int playerPos = TileManager.Instance.GetTileIndices(attacker.GetComponent<Tile>().CurrentTileData.transform);
        Vector2Int enemyPos = TileManager.Instance.GetTileIndices(target.GetComponent<Tile>().CurrentTileData.transform);

        WeaponData weaponData = gearEquipper.equippedWeapon;
        int splashDamage = weaponData.SplashDamage;
        float SplashAccuracy = weaponData.SplashAccuracy;

        if (playerPos.y == enemyPos.y)
        {
            // Same row → splash left & right
            TrySplashDamage(enemyPos.x - 1, enemyPos.y, splashDamage,SplashAccuracy);
            TrySplashDamage(enemyPos.x + 1, enemyPos.y, splashDamage,SplashAccuracy);
        }
        else if (playerPos.x == enemyPos.x)
        {
            // Same column → splash up & down
            TrySplashDamage(enemyPos.x, enemyPos.y - 1, splashDamage,SplashAccuracy);
            TrySplashDamage(enemyPos.x, enemyPos.y + 1, splashDamage,SplashAccuracy);
        }
        else
        {
            // Off-axis → pick orientation based on greater difference
            int rowDiff = Mathf.Abs(playerPos.y - enemyPos.y);
            int colDiff = Mathf.Abs(playerPos.x - enemyPos.x);

            if (rowDiff > colDiff)
            {
                // Vertical offset → splash left/right
                TrySplashDamage(enemyPos.x - 1, enemyPos.y, splashDamage,SplashAccuracy);
                TrySplashDamage(enemyPos.x + 1, enemyPos.y, splashDamage,SplashAccuracy);
            }
            else
            {
                // Horizontal offset → splash up/down
                TrySplashDamage(enemyPos.x, enemyPos.y - 1, splashDamage,SplashAccuracy);
                TrySplashDamage(enemyPos.x, enemyPos.y + 1, splashDamage,SplashAccuracy);
            }
        }
    }

    private void TrySplashDamage(int col, int row, int damage,float SplashAccuracy)
    {
        TileData tile = TileManager.Instance.GetTileDataAt(col, row);
        if (tile != null && tile.occupant != null)
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
