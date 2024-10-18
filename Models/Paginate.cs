namespace Shopping.Models
{
    public class Paginate
    {
        public int TotalItems {  get;private set; } // tổng số item 
        public int PageSize { get; private set; } // tổng số item/trang 
        public int CurrentPage {  get; private set; }// trang hiện tại 


        public int TotalPages { get; private set; }// tổng số trang 

        public int StartPage {  get; private set; }// trang bát đầu 
        public int EndPage { get; private set; }//  Trang kết thúc 

        public Paginate() { }


        public Paginate(int totalItems, int page, int pageSize =10) //10 items /1 trang
        {
            int totalPages = (int)Math.Ceiling((decimal)totalItems/(decimal)pageSize);
            int currentPage = page;

            int startPage = currentPage - 5; // trang bat dau tru 5 button 
            int endPage = currentPage + 4;
            
            if(startPage <= 0){
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }
            if(endPage > totalPages)
            {
                endPage = totalPages;
                if(endPage > 10)
                {
                    startPage = endPage - 9;
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize= pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;

        }
    }
}
