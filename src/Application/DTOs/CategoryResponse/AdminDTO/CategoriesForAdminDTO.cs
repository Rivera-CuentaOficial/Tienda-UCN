using System.ComponentModel.DataAnnotations;

namespace TiendaUCN.src.Application.DTOs.CategoryResponse.AdminDTO
{
    public class CategoriesForAdminDTO
    {
        public List<CategoryForAdminDTO> Categories { get; set; } = new List<CategoryForAdminDTO>();

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }
    }
}