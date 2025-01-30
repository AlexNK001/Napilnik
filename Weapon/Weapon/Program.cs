using System;

namespace Weapon
{
    public class Program
    {
        private static void Main()
        {
            Player player = new Player(health: 10);
            Weapon weapon = new Weapon(damage: 5);
            Bot bot = new Bot(weapon, bulletsCount: 3);
            bot.OnSeePlayer(player);
        }
    }

    public class Weapon
    {
        public Weapon(int damage)
        {
            if (damage > 0)
            {
                Damage = damage;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public int Damage { get; }
    }

    public class Player
    {
        private int _health;

        public Player(int health)
        {
            if (health > 0)
            {
                _health = health;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void TakeDamage(int damage)
        {
            int minDamage = 0;
            _health -= Math.Max(damage, minDamage);
        }
    }

    public class Bot
    {
        private readonly Weapon _weapon;
        private int _bullets;

        public Bot(Weapon weapon, int bulletsCount)
        {
            if (_bullets > 0)
            {
                _bullets = bulletsCount;
            }
            else
            {
                throw new InvalidOperationException();
            }
            
            _weapon = weapon;
        }

        public void OnSeePlayer(Player player)
        {
            if (_bullets > 0)
            {
                Fire(player);
            }
        }

        private void Fire(Player player)
        {
            player.TakeDamage(_weapon.Damage);
            _bullets--;
        }
    }
}
