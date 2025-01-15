namespace FINALPROJECT.DTO

{
     public class UsersToAddDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }

       public UsersToAddDto()
       {
        if(FirstName==null)
        {
            FirstName="";
        } if(LastName==null)
        {
            LastName="";
        } if(Email==null)
        {
            Email="";
        }
        
            if (PasswordSalt == null)
            {
                PasswordSalt = "";
            }
            if (PasswordHash == null)
            {
                PasswordHash = "";
            }
       }
    }
}