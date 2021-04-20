//using LogManager.Core.Entities;
//using LogManager.Core.Utilities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//namespace LogManager.Core.Settings
//{
//    public static class SearchPredicates
//    {
//        public static Expression<Func<BaseEntity, bool>> Get(BaseEntity entity, string searchText)
//        {
//            switch (entity)
//            {
//                case Ip _:
//                    Expression<Func<Ip, bool>> item = x => x.OwnerName.Contains(searchText)
//                                                        || IpConverter.FromBytes(x.Address).Contains(searchText);
//                    return item;
                    
//            }
//        }
//    }
//}
