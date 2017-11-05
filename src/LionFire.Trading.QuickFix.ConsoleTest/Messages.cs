using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using QuickFix;

namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public static class FieldMapExtensions
    {
        public static Func<T, R> GetFieldAccessor<T, R>(string fieldName)
        {
            ParameterExpression param =
            Expression.Parameter(typeof(T), "arg");

            MemberExpression member =
            Expression.Field(param, fieldName);

            LambdaExpression lambda =
            Expression.Lambda(typeof(Func<T, R>), member, param);

            Func<T, R> compiled = (Func<T, R>)lambda.Compile();
            return compiled;
        }

        /// <summary>Signature for a method which sets a specific member of a reference type</summary>
        /// <typeparam name="T">Type of the reference-type we're fondling</typeparam>
        /// <typeparam name="V">Type of the value we're setting</typeparam>
        /// <param name="this">The object instance to fondle</param>
        /// <param name="value">The new value to set the member to</param>
        public delegate void ReferenceTypeMemberSetterDelegate<T, V>(T @this, V value);

        /// <summary>Generate a specific member setter for a specific reference type</summary>
        /// <typeparam name="T">The type which contains the member</typeparam>
        /// <typeparam name="V">The member's actual type</typeparam>
        /// <param name="member_name">The member's name as defined in <typeparamref name="T"/></param>
        /// <returns>A compiled lambda which can access (set) the member</returns>
        public static ReferenceTypeMemberSetterDelegate<T, V> GenerateReferenceTypeMemberSetter<T, V>(string member_name)
            where T : class
        {
            var param_this = Expression.Parameter(typeof(T), "this");
            var param_value = Expression.Parameter(typeof(V), "value");				// the member's new value
            var member = Expression.PropertyOrField(param_this, member_name);	// i.e., 'this.member_name'
            var assign = Expression.Assign(member, param_value);				// i.e., 'this.member_name = value'
            var lambda = Expression.Lambda<ReferenceTypeMemberSetterDelegate<T, V>>(assign, param_this, param_value);

            return lambda.Compile();
        }

        private static Func<FieldMap, SortedDictionary<int, QuickFix.Fields.IField>> GetFields = GetFieldAccessor<FieldMap, SortedDictionary<int, QuickFix.Fields.IField>>("_fields");
        private static ReferenceTypeMemberSetterDelegate<FieldMap, SortedDictionary<int, QuickFix.Fields.IField>> SetFields = GenerateReferenceTypeMemberSetter<FieldMap, SortedDictionary<int, QuickFix.Fields.IField>>("_fields");

        private static Func<FieldMap, Dictionary<int, List<Group>>> GetGroups = GetFieldAccessor<FieldMap, Dictionary<int, List<Group>>>("_groups");
        private static ReferenceTypeMemberSetterDelegate<FieldMap, Dictionary<int, List<Group>>> SetGroups = GenerateReferenceTypeMemberSetter<FieldMap, Dictionary<int, List<Group>>>("_groups");

        private static ReferenceTypeMemberSetterDelegate<FieldMap, List<QuickFix.Fields.IField>> SetRepeatedTags = GenerateReferenceTypeMemberSetter<FieldMap, List<QuickFix.Fields.IField>>("RepeatedTags");

        public static void CopyStateFrom(this FieldMap fmTo, FieldMap src)
        {
            Array.Copy(src.FieldOrder, fmTo.FieldOrder, src.FieldOrder.Length);
            SetFields(fmTo, GetFields(src));
            var newGroups = new Dictionary<int, List<Group>>();
            var groups = GetGroups(src);
            foreach (KeyValuePair<int, List<Group>> g in groups)
                newGroups.Add(g.Key, new List<Group>(g.Value));
            SetGroups(fmTo, newGroups);
            SetRepeatedTags(fmTo, src.RepeatedTags);
        }
    }
}    
// Generated helper templates
// Generated items
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\BusinessMessageReject.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\ListStatusRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\ListCancelRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\ListExecute.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\ListStatus.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\ListStrikePrice.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\NewOrderList.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\BidResponse.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\BidRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\SettlementInstructions.cs
// AllocationACK.cs
// Allocation.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\OrderStatusRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\OrderCancelReject.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\OrderCancelRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\OrderCancelReplaceRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\DontKnowTrade.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\ExecutionReport.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\NewOrderSingle.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\TradingSessionStatus.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\TradingSessionStatusRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\SecurityStatus.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\SecurityStatusRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\SecurityDefinition.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\SecurityDefinitionRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\MarketDataRequestReject.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\MarketDataIncrementalRefresh.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\MarketDataSnapshotFullRefresh.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\MarketDataRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\QuoteAcknowledgement.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\QuoteStatusRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\QuoteCancel.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\MassQuote.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\Quote.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\QuoteRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\Email.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\News.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\IndicationofInterest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\Advertisement.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\Logout.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\SequenceReset.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\Reject.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\ResendRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\TestRequest.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\Logon.cs
// C:\Src\Trading\src\LionFire.Trading.QuickFix.ConsoleTest\Heartbeat.cs


