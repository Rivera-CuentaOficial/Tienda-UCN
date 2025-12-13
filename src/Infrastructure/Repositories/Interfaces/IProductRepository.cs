using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Domain.Models;

namespace TiendaUCN.src.Infrastructure.Repositories.Interfaces;

public interface IProductRepository
{
    /// <summary>
    /// Crea un nuevo producto en la base de datos.
    /// </summary>
    /// <param name="product">El producto a crear.</param>
    /// <returns>Id del producto creado.</returns>
    Task<int> CreateAsync(Product product);
    /// <summary>
    /// Actualiza un producto existente en la base de datos.
    /// </summary>
    /// <param name="existingProduct">El producto existente a actualizar.</param>
    /// <returns>Indica si la actualización fue exitosa.</returns>
    Task<bool> UpdateAsync(Product existingProduct);
    /// <summary>
    /// Obtiene productos filtrados para el administrador.
    /// </summary>
    /// <param name="searchParams">Parámetros de búsqueda para filtrar productos.</param>
    /// <returns>Productos filtrados para el administrador y el conteo total.</returns>
    Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForAdminAsync(
        SearchParamsDTO searchParams
    );
    /// <summary>
    /// Obtiene productos filtrados para el cliente.
    /// </summary>
    /// <param name="searchParams">Parámetros de búsqueda para filtrar productos.</param>
    /// <returns>Productos filtrados para el cliente y el conteo total.</returns>
    Task<(IEnumerable<Product> products, int totalCount)> GetFilteredForCustomerAsync(
        SearchParamsDTO searchParams
    );
    /// <summary>
    /// Obtiene un producto por su ID.
    /// </summary>
    /// <param name="id">ID del producto.</param>
    /// <param name="asTracking">Indica si la consulta debe realizarse con seguimiento de cambios.</param>
    /// <returns>El producto correspondiente al ID o null si no se encuentra.</returns>
    Task<Product?> GetByIdAsync(int id, bool asTracking = false);
    /// <summary>
    /// Obtiene un producto por su ID para el administrador.
    /// </summary>
    /// <param name="id">ID del producto.</param>
    /// <param name="asTracking">Indica si la consulta debe realizarse con seguimiento de cambios.</param>
    /// <returns>El producto correspondiente al ID o null si no se encuentra.</returns>
    Task<Product?> GetByIdForAdminAsync(int id, bool asTracking = false);
    /// <summary>
    /// Alterna el estado de disponibilidad de un producto.
    /// </summary>
    /// <param name="id">ID del producto.</param>
    /// <returns></returns>
    Task<bool> ToggleActiveAsync(Product product);
    /// <summary>
    /// Obtiene el stock real de un producto.
    /// </summary>
    /// <param name="productId">ID del producto.</param>
    /// <returns>El stock real del producto.</returns>
    Task<int> GetRealStockAsync(int productId);
    /// <summary>
    /// Actualiza el stock de un producto.
    /// </summary>
    /// <param name="productId">ID del producto.</param>
    /// <param name="stock">Nuevo valor de stock.</param>
    /// <returns></returns>
    Task UpdateStockAsync(int productId, int stock);
    /// <summary>
    /// Cuenta la cantidad de productos en una categoría específica.
    /// </summary>
    /// <param name="categoryId">ID de la categoría.</param>
    /// <returns>Cantidad de productos en la categoría.</returns>
    Task<int> CountProductsByCategoryIdAsync(int categoryId);
    /// <summary>
    /// Cuenta la cantidad de productos de una marca específica.
    /// </summary>
    /// <param name="brandId">ID de la marca.</param>
    /// <returns>Cantidad de productos de la marca.</returns>
    Task<int> CountProductsByBrandIdAsync(int brandId);
    /// <summary>
    /// Elimina un producto de la base de datos.
    /// </summary>
    /// <param name="product">El producto a eliminar.</param>
    /// <returns>Indica si la eliminación fue exitosa.</returns>
    Task<bool> DeleteAsync(Product product);
}