using System;

namespace Weapon
{
    public class Program
    {
        private static void Main()
        {
            Player player = new Player(health: 10);
            Weapon weapon = new Weapon(damage: 5, bulletsCount: 3);
            Bot bot = new Bot(weapon);
            bot.OnSeePlayer(player);
        }
    }

    public class Weapon
    {
        private readonly int _damage;
        private int _bullets;

        public Weapon(int damage, int bulletsCount)
        {
            if (damage > 0)
                _damage = damage;
            else
                throw new ArgumentOutOfRangeException();

            if (bulletsCount > 0)
                _bullets = bulletsCount;
            else
                throw new ArgumentOutOfRangeException();
        }

        public void Fire(Player player)
        {
            if (_bullets > 0)
            {
                player.TakeDamage(_damage);
                _bullets--;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }

    public class Player
    {
        private int _health;

        public Player(int health)
        {
            if (health > 0)
                _health = health;
            else
                throw new ArgumentOutOfRangeException();
        }

        public void TakeDamage(int damage)
        {
            if (damage > 0)
                _health -= damage;
            else
                throw new ArgumentOutOfRangeException();
        }
    }

    public class Bot
    {
        private readonly Weapon _weapon;

        public Bot(Weapon weapon)
        {
            if (weapon != null)
                _weapon = weapon;
            else
                throw new ArgumentNullException();
        }

        public void OnSeePlayer(Player player)
        {
            _weapon.Fire(player);
        }
    }
}