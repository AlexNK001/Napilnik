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
            _damage = damage > 0 ? damage : throw new ArgumentOutOfRangeException(nameof(damage));
            _bullets = bulletsCount > 0 ? bulletsCount : throw new ArgumentOutOfRangeException(nameof(bulletsCount));
        }

        public void Fire(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            if (_bullets < 0)
                throw new InvalidOperationException(nameof(_bullets));

            player.TakeDamage(_damage);
            _bullets--;
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
                throw new ArgumentOutOfRangeException(nameof(health));
        }

        public void TakeDamage(int damage)
        {
            if (damage > 0)
                _health -= damage;
            else
                throw new ArgumentOutOfRangeException(nameof(damage));
        }
    }

    public class Bot
    {
        private readonly Weapon _weapon;

        public Bot(Weapon weapon)
        {
            _weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
        }

        public void OnSeePlayer(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            _weapon.Fire(player);
        }
    }
}