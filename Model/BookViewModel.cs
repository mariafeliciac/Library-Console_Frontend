using Model.Interfaces;

namespace Model
{
    public class BookViewModel
    {
        public IBook Book { get; set; }
        public List<ReservationViewModel> ReservationsViewModel { get; set; }

        public AvailabilityBook AvailabilityBook {  get; set; }
        public DateTime AvailabilityDate {  get; set; }


    }
}
