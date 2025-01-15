namespace FINALPROJECT.DTO
{
    public partial class UserForLoginConformationDto
    {
         public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public bool is_active { get; set; }
        UserForLoginConformationDto(){
        
            if (PasswordHash == null)
            {
                PasswordHash = new byte[0];
            }
            if (PasswordSalt == null)
            {
                PasswordSalt = new byte[0];
            }  }


    }
}