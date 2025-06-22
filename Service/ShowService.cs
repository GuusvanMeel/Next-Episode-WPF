using Interfaces.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class ShowService
    {
        private IShowRepo Repo;
        public ShowService(IShowRepo _repo)
        {
            this.Repo = _repo;
        }
        //addshow you need to check if a show with that name already exists before adding, overwriting it.
    }
}
