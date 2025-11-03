using Microsoft.EntityFrameworkCore;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Data;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Infrastructure.Repositories.Implements
{
    public class CartRepository : ICartRepository
    {
        private readonly DataContext _context;

        public CartRepository(DataContext dataContext)
        {
            _context = dataContext;
        }

        /// <summary>
        /// Busca un carrito por buyerId y userId
        /// </summary>
        public async Task<Cart?> FindAsync(string buyerId, int? userId)
        {
            Cart? cart = null;

            if (userId.HasValue)
            {
                cart = await _context
                    .Carts.Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Images)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart != null)
                {
                    if (cart.BuyerId != buyerId)
                    {
                        cart.BuyerId = buyerId;
                        cart.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    return cart;
                }
            }

            cart = await _context
                .Carts.Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.BuyerId == buyerId && c.UserId == null);

            if (cart != null && userId.HasValue)
            {
                cart.UserId = userId;
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        /// <summary>
        /// Busca un carrito por userId únicamente
        /// </summary>
        public async Task<Cart?> GetByUserIdAsync(int userId)
        {
            return await _context
                .Carts.Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        /// <summary>
        /// Busca un carrito anónimo por buyerId
        /// </summary>
        public async Task<Cart?> GetAnonymousAsync(string buyerId)
        {
            return await _context
                .Carts.Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.BuyerId == buyerId && c.UserId == null);
        }

        /// <summary>
        /// Crea un nuevo carrito
        /// </summary>
        public async Task<Cart> CreateAsync(string buyerId, int? userId = null)
        {
            var cart = new Cart
            {
                BuyerId = buyerId,
                UserId = userId,
                SubTotal = 0,
                Total = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return await _context
                .Carts.Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Images)
                .FirstAsync(c => c.Id == cart.Id);
        }

        /// <summary>
        /// Actualiza un carrito existente
        /// </summary>
        public async Task UpdateAsync(Cart cart)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina un carrito
        /// </summary>
        public async Task DeleteAsync(Cart cart)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Agrega un CartItem a un carrito
        /// </summary>
        public async Task AddItemAsync(Cart cart, CartItem cartItem)
        {
            cart.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Elimina un CartItem
        /// </summary>
        public async Task RemoveItemAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }
    }
}