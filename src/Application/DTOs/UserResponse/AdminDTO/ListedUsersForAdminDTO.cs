namespace TiendaUCN.src.Application.DTOs.UserResponse.AdminDTO
{
    public class ListedUsersForAdminDTO
    {
        public List<UserForAdminDTO> Users { get; set; } = new List<UserForAdminDTO>();

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }
    }
}