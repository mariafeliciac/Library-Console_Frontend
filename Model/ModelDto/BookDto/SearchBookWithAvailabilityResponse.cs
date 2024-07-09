namespace Model.ModelDto.BookDto
{
    public class SearchBookWithAvailabilityeResponse : BookSearchResponse
    {

        public AvailabilityBook AvailabilityBook { get; set; }

        public DateTime AvailabilityDate {  get; set; }




    }
}
