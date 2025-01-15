namespace FINALPROJECT.DTO
{
     public class UsersToEditDto
    {
        public int userid{get; set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
       

       public UsersToEditDto()
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
       }
    }
}