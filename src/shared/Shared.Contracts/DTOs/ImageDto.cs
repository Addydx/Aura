//Esto es como un contenedor para los DTOs relacionados con imágenes 
//que es Dtos(Data Transfer Objects) y se utliza para transferir datos entre 
//diferentes capas de una aplicacion.
namespace Shared.Contracts.DTOs
{
    public class ImageDtos
    {
        //Representa un DTO para una imagen
        public string Id { get; set; }
        //Representa la URL de la imagen
        public string Url { get; set; }
    }
}