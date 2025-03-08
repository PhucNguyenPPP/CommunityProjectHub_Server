using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPH.Common.DTO.Trainee
{
    public class ScoreTraineeRequestDTO
    {
       public List<ScoreTraineeDTO> ScoreTrainees {get; set;} = new List<ScoreTraineeDTO>();
       public Guid ClassId { get; set;}
    }
}
