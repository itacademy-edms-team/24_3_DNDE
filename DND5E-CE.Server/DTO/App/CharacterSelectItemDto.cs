namespace DND5E_CE.Server.DTO.App
{
    public class CharacterSelectItemDto
    {
        public Guid Id { get; set; } // Character Id

        public string Name { get; set; }

        public int Level { get; set; }

        public string Class { get; set; }
    }
}
