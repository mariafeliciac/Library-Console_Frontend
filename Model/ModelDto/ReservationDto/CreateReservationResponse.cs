using Model.Interfaces;
using Model.Model;

namespace Model.ModelDto.ReservationDto
{
    public class CreateReservationResponse
    {
        public Book Book { get; set; }
        public User User { get; set; }
        public DateTime EndDateReservation { get; set; }
        public bool Result { get; set; }
    }
}
