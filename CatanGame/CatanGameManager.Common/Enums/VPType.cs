using System;

namespace CatanGameManager.CommonObjects.Enums
{
    public class VPType
    {
        public VPType(UpdateType typeToUpdate, InterChanageableVP interChanageableVp)
        {
            TypeToUpdate = typeToUpdate;
            if ((typeToUpdate == UpdateType.Interchangeable) && interChanageableVp == InterChanageableVP.None){
                throw new Exception();
            }

            TypeOfInterchangeable = interChanageableVp;
        }

        public VPType(UpdateType typeToUpdate)
        {
            TypeToUpdate = typeToUpdate;
        }


        public enum UpdateType
        {
            Settlment,
            City,
            SaviorOfCatan,
            Printer,
            Constitution,
            Interchangeable
        }

        public enum InterChanageableVP
        {
            None,
            LongestRoad,
            MetropolisCloth,
            MetropolisCoin,
            MetropolisPaper,
            Merchant            
        }

        public UpdateType TypeToUpdate { get; private set; }

        public InterChanageableVP TypeOfInterchangeable {get; private set; }
    }
  
}
