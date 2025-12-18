using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductStoreDesktop.Services
{
    public static class CartService
    {
        private static readonly Dictionary<int, int> _cart = new Dictionary<int, int>();

        public static void AddItem(int productId)
        {
            if (_cart.ContainsKey(productId))
                _cart[productId]++;
            else
                _cart[productId] = 1;
        }

        public static void RemoveItem(int productId)
        {
            _cart.Remove(productId);
        }

        public static void UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
                _cart.Remove(productId);
            else
                _cart[productId] = quantity;
        }

        public static Dictionary<int, int> GetCart() => new Dictionary<int, int>(_cart);

        public static void Clear() => _cart.Clear();
    }
}
