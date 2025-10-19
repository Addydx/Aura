//Esto es como un contenedor para los DTOs relacionados con imágenes 
//que es Dtos(Data Transfer Objects) y se utliza para transferir datos entre 
//diferentes capas de una aplicacion.
namespace Shared.Contracts.DTOs
{
    public class ImageDtos
    {
        //Representa un DTO para una imagen
        public string Id { get; set; } = string.Empty;
        //Representa la URL de la imagen
        public string Url { get; set; } = string.Empty;
        //OwnerId
        public string OwnerId { get; set; } = string.Empty;
        //Description
        public string Description { get; set; } = string.Empty;
        //Datetime UploadedAt
        public DateTime UploadedAt { get; set; }
        //string.Empy es un valor predeterminado para cadenas vacias
    }
}