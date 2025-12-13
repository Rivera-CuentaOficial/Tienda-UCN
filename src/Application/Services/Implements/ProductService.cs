using Mapster;
using Serilog;
using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Application.DTOs.ProductResponse.AdminDTO;
using TiendaUCN.src.Application.DTOs.ProductResponse.CustomerDTO;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;
    private readonly int _defaultPageSize;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IBrandRepository brandRepository, IConfiguration configuration, IFileService fileService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _brandRepository = brandRepository;
        _configuration = configuration;
        _fileService = fileService;
        _defaultPageSize =
            _configuration.GetValue<int?>("Products:DefaultPageSize")
            ?? throw new ArgumentNullException(
                "La configuración 'DefaultPageSize' no está definida."
            );
    }

    /// <summary>
    /// Obtiene productos filtrados para administradores.
    /// </summary>
    /// <param name="searchParams">Parámetros de búsqueda para filtrar productos.</param>
    /// <returns>Productos filtrados para administradores.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task<ListedProductsForAdminDTO> GetFilteredForAdminAsync(
        SearchParamsDTO searchParams
    )
    {
        Log.Information(
            "Obteniendo productos para administrador con parámetros de búsqueda: {@SearchParams}",
            searchParams
        );
        var (products, totalCount) = await _productRepository.GetFilteredForAdminAsync(
            searchParams
        );
        var totalPages = (int)
            Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
        int currentPage = searchParams.PageNumber;
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        if (currentPage < 1 || currentPage > totalPages)
        {
            throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
        }
        Log.Information(
            "Total de productos encontrados: {TotalCount}, Total de páginas: {TotalPages}, Página actual: {CurrentPage}, Tamaño de página: {PageSize}",
            totalCount,
            totalPages,
            currentPage,
            pageSize
        );

        // Convertimos los productos filtrados a DTOs para la respuesta
        return new ListedProductsForAdminDTO
        {
            Products = products.Adapt<List<ProductForAdminDTO>>(),
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            PageSize = products.Count(),
        };
    }

    public async Task<ListedProductsForCustomerDTO> GetFilteredForCustomerAsync(
        SearchParamsDTO searchParams
    )
    {
        var (products, totalCount) = await _productRepository.GetFilteredForCustomerAsync(
            searchParams
        );
        var totalPages = (int)
            Math.Ceiling((double)totalCount / (searchParams.PageSize ?? _defaultPageSize));
        int currentPage = searchParams.PageNumber;
        int pageSize = searchParams.PageSize ?? _defaultPageSize;
        if (currentPage < 1 || currentPage > totalPages)
        {
            throw new ArgumentOutOfRangeException("El número de página está fuera de rango.");
        }
        Log.Information(
            "Total de productos encontrados: {TotalCount}, Total de páginas: {TotalPages}, Página actual: {CurrentPage}, Tamaño de página: {PageSize}",
            totalCount,
            totalPages,
            currentPage,
            pageSize
        );

        // Convertimos los productos filtrados a DTOs para la respuesta
        return new ListedProductsForCustomerDTO
        {
            Products = products.Adapt<List<ProductForCustomerDTO>>(),
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            PageSize = products.Count(),
        };
    }

    public async Task<ProductDetailDTO> GetByIdAsync(int id)
    {
        var product =
            await _productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
        Log.Information("Producto encontrado: {@Product}", product);
        return product.Adapt<ProductDetailDTO>();
    }

    public async Task<ProductDetailForAdminDTO> GetByIdForAdminAsync(int id)
    {
        var product =
            await _productRepository.GetByIdForAdminAsync(id)
            ?? throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
        Log.Information("Producto encontrado: {@Product}", product);
        return product.Adapt<ProductDetailForAdminDTO>();
    }

    public async Task<string> CreateAsync(CreateProductDTO createProductDTO)
    {
        Product product = createProductDTO.Adapt<Product>();
        Category category =
            await _categoryRepository.CreateOrGetCategoryAsync(createProductDTO.CategoryName)
            ?? throw new Exception("Error al crear o obtener la categoría del producto.");
        Brand brand =
            await _brandRepository.CreateOrGetBrandAsync(createProductDTO.BrandName)
            ?? throw new Exception("Error al crear o obtener la marca del producto.");
        product.CategoryId = category.Id;
        product.BrandId = brand.Id;
        product.Images = new List<Image>();
        int productId = await _productRepository.CreateAsync(product);
        Log.Information("Producto creado: {@Product}", product);

        // Si se enviaron imágenes en el DTO las subimos a Cloudinary y las asociamos al producto
        if (createProductDTO.Images == null || !createProductDTO.Images.Any())
        {
            Log.Information("No se proporcionaron imágenes. No se subirán imágenes al crear el producto.");
        }
        else
        {
            foreach (var image in createProductDTO.Images)
            {
                try
                {
                    Log.Information("Subiendo imagen asociada al producto: {FileName}", image.FileName);
                    await _fileService.UploadAsync(image, productId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error subiendo la imagen {FileName} para el producto {ProductId}", image.FileName, productId);
                    throw; // Dejar que el middleware de excepciones maneje la respuesta
                }
            }
        }

        return product.Id.ToString();
    }

    public async Task<string> UpdateAsync(int id, UpdateProductDTO updateProductDTO)
    {
        Product existingProduct =
            await _productRepository.GetByIdForAdminAsync(id, true)
            ?? throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");

        // Actualizar la categoría si es necesario
        if (existingProduct.Category.Name != updateProductDTO.CategoryName && updateProductDTO.CategoryName != null)
        {
            // Verificar si la nueva categoría existe
            var category = await _categoryRepository.GetByNameAsync(updateProductDTO.CategoryName);
            if (category == null)
            {
                Log.Warning($"La categoría con nombre {updateProductDTO.CategoryName} no existe.");
                category = await _categoryRepository.CreateOrGetCategoryAsync(updateProductDTO.CategoryName);
            }

            existingProduct.CategoryId = category.Id;
            existingProduct.Category = category;
        }

        // Actualizar la marca si es necesario
        if (existingProduct.Brand.Name != updateProductDTO.BrandName && updateProductDTO.BrandName != null)
        {
            // Verificar si la nueva marca existe
            var brand = await _brandRepository.GetByNameAsync(updateProductDTO.BrandName);
            if (brand == null)
            {
                Log.Warning($"La marca con nombre {updateProductDTO.BrandName} no existe.");
                brand = await _brandRepository.CreateOrGetBrandAsync(updateProductDTO.BrandName);
            }
            existingProduct.BrandId = brand.Id;
            existingProduct.Brand = brand;
        }

        // Actualizar las imagenes si se proporcionan nuevas imágenes
        if (updateProductDTO.Images != null && updateProductDTO.Images.Any())
        {
            // Eliminar las imágenes existentes del producto
            if (existingProduct.Images != null && existingProduct.Images.Any())
            {
                var images = existingProduct.Images.ToList();
                foreach (var image in images)
                {
                    Log.Information("Eliminando imagen existente asociada al producto: {ImageUrl}", image.ImageUrl);
                    var result = await _fileService.DeleteAsync(image.PublicId);
                    if (!result)
                    {
                        Log.Warning("No se pudo eliminar la imagen: {ImageUrl}", image.ImageUrl);
                        throw new Exception($"No se pudo eliminar la imagen: {image.ImageUrl}");
                    }
                }
                existingProduct.Images.Clear();
            }

            // Subir las nuevas imágenes y asociarlas al producto
            foreach (var image in updateProductDTO.Images)
            {
                Log.Information("Subiendo nueva imagen asociada al producto: {FileName}", image.FileName);
                var result = await _fileService.UploadAsync(image, existingProduct.Id);
                if (!result)
                {
                    Log.Warning("No se pudo subir la imagen: {FileName}", image.FileName);
                    throw new Exception($"No se pudo subir la imagen: {image.FileName}");
                }
            }
        }

        // Actualizar las propiedades del producto existente con los valores del DTO
        if (updateProductDTO.Title != null && existingProduct.Title != updateProductDTO.Title)
            existingProduct.Title = updateProductDTO.Title;
        if (updateProductDTO.Description != null && existingProduct.Description != updateProductDTO.Description)
            existingProduct.Description = updateProductDTO.Description;
        if (updateProductDTO.Price.HasValue && existingProduct.Price != updateProductDTO.Price.Value)
            existingProduct.Price = updateProductDTO.Price.Value;
        if (updateProductDTO.Stock.HasValue && existingProduct.Stock != updateProductDTO.Stock.Value)
            existingProduct.Stock = updateProductDTO.Stock.Value;

        await _productRepository.UpdateAsync(existingProduct);
        Log.Information("Producto actualizado: {@Product}", existingProduct);

        return existingProduct.Id.ToString();
    }

    public async Task<string> ToggleActiveAsync(int id)
    {
        var product =
            await _productRepository.GetByIdForAdminAsync(id, true)
            ?? throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");

        var result = await _productRepository.ToggleActiveAsync(product);
        if (!result) throw new Exception("No se pudo actualizar el estado del producto.");
        Log.Information("Estado del producto actualizado: {@Product}", product);
        return "Estado del producto actualizado correctamente.";
    }

    public async Task<string> DeleteAsync(int id)
    {
        var product =
            await _productRepository.GetByIdForAdminAsync(id, true)
            ?? throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");

        var deleted = await _productRepository.DeleteAsync(product);
        if (!deleted) throw new Exception("No se pudo eliminar el producto.");

        Log.Information("Producto eliminado: {@Product}", product);

        return "Producto eliminado correctamente.";
    }

    public async Task<string> UpdateProductDiscountAsync(int id, UpdateProductDiscountDTO updateProductDiscountDTO)
    {
        Product existingProduct =
            await _productRepository.GetByIdForAdminAsync(id, true)
            ?? throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");

        existingProduct.Discount = updateProductDiscountDTO.Discount;

        await _productRepository.UpdateAsync(existingProduct);
        Log.Information("Descuento del producto actualizado: {@Product}", existingProduct);

        return "Descuento del producto actualizado correctamente.";
    }
}