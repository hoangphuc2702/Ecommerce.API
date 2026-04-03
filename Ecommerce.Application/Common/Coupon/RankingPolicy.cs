using Ecommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Common.Coupon
{
    public static class RankingPolicy
    {
        public const decimal MoneyPerPoint = 100_000m;

        public static int CalculateEarnedPoints(decimal orderTotalAmount)
        {
            if (orderTotalAmount <= 0) return 0;
            return (int)(orderTotalAmount / MoneyPerPoint);
        }

        public static MembershipRank CalculateRank(int totalPoints)
        {
            if (totalPoints >= 1000) return MembershipRank.Diamond;
            if (totalPoints >= 500) return MembershipRank.Gold;     
            if (totalPoints >= 100) return MembershipRank.Silver;   

            return MembershipRank.Member;
        }

        public static decimal GetDiscountPercentage(MembershipRank rank)
        {
            return rank switch
            {
                MembershipRank.Diamond => 0.10m,
                MembershipRank.Gold => 0.05m,
                MembershipRank.Silver => 0.02m,
                _ => 0m                          
            };
        }
    }
}
