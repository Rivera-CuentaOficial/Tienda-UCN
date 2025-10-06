using Mapster;
using Serilog;
using TiendaUCN.src.Application.DTOs.ProductResponse;
using TiendaUCN.src.Application.DTOs.ProductResponse.Admin;
using TiendaUCN.src.Application.DTOs.ProductResponse.Customer;
using TiendaUCN.src.Application.Services.Interfaces;
using TiendaUCN.src.Domain.Models;
using TiendaUCN.src.Infrastructure.Repositories.Interfaces;

namespace TiendaUCN.src.Application.Services.Implements;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IConfiguration _configuration;
    private readonly int _defaultPageSize;

    public ProductService(IProductRepository productRepository, IConfiguration configuration)
    {
        _productRepository = productRepository;
        _configuration = configuration;
        _defaultPageSize =
            _configuration.GetValue<int?>("Products:DefaultPageSize")
            ?? throw new ArgumentNullException(
                "La configuración 'DefaultPageSize' no está definida."
            );
    }

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

    public async Task<ProductDetailDTO> GetByIdForAdminAsync(int id)
    {
        var product =
            await _productRepository.GetByIdForAdminAsync(id)
            ?? throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
        Log.Information("Producto encontrado: {@Product}", product);
        return product.Adapt<ProductDetailDTO>();
    }

    public async Task<string> CreateAsync(CreateProductDTO createProductDTO)
    {
        Product product = createProductDTO.Adapt<Product>();
        Category category =
            await _productRepository.CreateOrGetCategoryAsync(createProductDTO.CategoryName)
            ?? throw new Exception("Error al crear o obtener la categoría del producto.");
        Brand brand =
            await _productRepository.CreateOrGetBrandAsync(createProductDTO.BrandName)
            ?? throw new Exception("Error al crear o obtener la marca del producto.");
        product.CategoryId = category.Id;
        product.BrandId = brand.Id;
        product.Images = new List<Image>();
        int productId = await _productRepository.CreateAsync(product);
        Log.Information("Producto creado: {@Product}", product);
        //TODO
        /*if (createProductDTO.Images == null || !createProductDTO.Images.Any())
        {
            Log.Information("No se proporcionaron imágenes. Se asignará la imagen por defecto.");
            throw new InvalidOperationException(
                "Debe proporcionar al menos una imagen para el producto."
            );
        }
        foreach (var image in createProductDTO.Images)
        {
            Log.Information("Imagen asociada al producto: {@Image}", image);
            await _fileService.UploadAsync(image, productId);
        }*/
        return product.Id.ToString();
    }

    public async Task ToggleActiveAsync(int id)
    {
        await _productRepository.ToggleActiveAsync(id);
    }
}