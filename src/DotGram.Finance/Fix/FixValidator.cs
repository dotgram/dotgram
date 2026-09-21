using System.Collections.Generic;

namespace DotGram.Finance.Fix;

/// <summary>Checks one message and adds to <paramref name="findings"/> whatever is wrong with it.</summary>
/// <param name="message">The message to check.</param>
/// <param name="findings">Where to put what is wrong; it may already hold findings of another rule.</param>
/// <remarks>
/// <para>
/// One shape throughout, taking the base message, because a rule written for a type is reached
/// through that type's own field and already knows which it is.
/// </para>
/// <para>
/// A rule is called once per message and may be called from several threads at once, so it must not
/// keep state between calls. It adds and does not clear: what is already in the list is another
/// rule's answer about the same message.
/// </para>
/// </remarks>
public delegate void FixMessageRule(FixMessage message, List<FixFinding> findings);

/// <summary>
/// The rules this package compiles in, one named method a message type.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Not a way in.</strong> Nothing here is called to validate a message — that is
/// <see cref="FixMessage.Validate"/>, and what it asks is the <c>Rule</c> field of the message's
/// own class. What this class is for is the NAME: a consumer who replaces
/// <c>FixMessage.NewOrderSingle.Rule</c> can put the package's own rule back by assigning
/// <see cref="ValidateNewOrderSingle"/> to it, which is what makes a replacement undoable.
/// </para>
/// <para>
/// Every one of them holds the message to FIX 4.4 as this package compiles it: required fields and
/// components, the order and uniqueness of a group entry's fields, primitive syntax, code sets, and
/// the rule that <c>MessageEncoding</c> accompanies an <c>Encoded</c> field. They are one walk over
/// one set of tables, reached by ninety-four names, because the name is the point and a second copy
/// of the rules would be a second thing to disagree with the first.
/// </para>
/// </remarks>
public static class FixValidator
{
	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.Custom"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateCustom(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.Heartbeat"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateHeartbeat(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.TestRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateTestRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.ResendRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateResendRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.Reject"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateReject(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.SequenceReset"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateSequenceReset(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.Logout"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateLogout(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.IOI"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateIOI(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.Advertisement"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateAdvertisement(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.ExecutionReport"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateExecutionReport(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.OrderCancelReject"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateOrderCancelReject(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.Logon"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateLogon(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.News"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateNews(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.Email"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateEmail(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.NewOrderSingle"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateNewOrderSingle(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.NewOrderList"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateNewOrderList(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.OrderCancelRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateOrderCancelRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.OrderCancelReplaceRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateOrderCancelReplaceRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.OrderStatusRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateOrderStatusRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.AllocationInstruction"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateAllocationInstruction(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.ListCancelRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateListCancelRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.ListExecute"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateListExecute(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.ListStatusRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateListStatusRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.ListStatus"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateListStatus(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.AllocationInstructionAck"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateAllocationInstructionAck(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.DontKnowTrade"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateDontKnowTrade(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.QuoteRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateQuoteRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.Quote"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateQuote(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.SettlementInstructions"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateSettlementInstructions(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.MarketDataRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateMarketDataRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.MarketDataSnapshotFullRefresh"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateMarketDataSnapshotFullRefresh(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.MarketDataIncrementalRefresh"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateMarketDataIncrementalRefresh(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.MarketDataRequestReject"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateMarketDataRequestReject(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.QuoteCancel"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateQuoteCancel(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.QuoteStatusRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateQuoteStatusRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.MassQuoteAcknowledgement"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateMassQuoteAcknowledgement(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.SecurityDefinitionRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateSecurityDefinitionRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.SecurityDefinition"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateSecurityDefinition(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.SecurityStatusRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateSecurityStatusRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.SecurityStatus"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateSecurityStatus(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.TradingSessionStatusRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateTradingSessionStatusRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.TradingSessionStatus"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateTradingSessionStatus(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.MassQuote"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateMassQuote(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.BusinessMessageReject"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateBusinessMessageReject(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.BidRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateBidRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.BidResponse"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateBidResponse(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.ListStrikePrice"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateListStrikePrice(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.XMLnonFIX"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateXMLnonFIX(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.RegistrationInstructions"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateRegistrationInstructions(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.RegistrationInstructionsResponse"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateRegistrationInstructionsResponse(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.OrderMassCancelRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateOrderMassCancelRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.OrderMassCancelReport"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateOrderMassCancelReport(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.NewOrderCross"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateNewOrderCross(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.CrossOrderCancelReplaceRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateCrossOrderCancelReplaceRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.CrossOrderCancelRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateCrossOrderCancelRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.SecurityTypeRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateSecurityTypeRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.SecurityTypes"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateSecurityTypes(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.SecurityListRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateSecurityListRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.SecurityList"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateSecurityList(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.DerivativeSecurityListRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateDerivativeSecurityListRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.DerivativeSecurityList"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateDerivativeSecurityList(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.NewOrderMultileg"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateNewOrderMultileg(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.MultilegOrderCancelReplace"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateMultilegOrderCancelReplace(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.TradeCaptureReportRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateTradeCaptureReportRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.TradeCaptureReport"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateTradeCaptureReport(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.OrderMassStatusRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateOrderMassStatusRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.QuoteRequestReject"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateQuoteRequestReject(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.RFQRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateRFQRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.QuoteStatusReport"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateQuoteStatusReport(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.QuoteResponse"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateQuoteResponse(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.Confirmation"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateConfirmation(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.PositionMaintenanceRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidatePositionMaintenanceRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.PositionMaintenanceReport"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidatePositionMaintenanceReport(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.RequestForPositions"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateRequestForPositions(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.RequestForPositionsAck"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateRequestForPositionsAck(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.PositionReport"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidatePositionReport(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.TradeCaptureReportRequestAck"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateTradeCaptureReportRequestAck(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.TradeCaptureReportAck"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateTradeCaptureReportAck(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.AllocationReport"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateAllocationReport(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.AllocationReportAck"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateAllocationReportAck(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.ConfirmationAck"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateConfirmationAck(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.SettlementInstructionRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateSettlementInstructionRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.AssignmentReport"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateAssignmentReport(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.CollateralRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateCollateralRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.CollateralAssignment"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateCollateralAssignment(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.CollateralResponse"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateCollateralResponse(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.CollateralReport"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateCollateralReport(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.CollateralInquiry"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateCollateralInquiry(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.NetworkCounterpartySystemStatusRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateNetworkCounterpartySystemStatusRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.NetworkCounterpartySystemStatusResponse"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateNetworkCounterpartySystemStatusResponse(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.UserRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateUserRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.UserResponse"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateUserResponse(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.CollateralInquiryAck"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateCollateralInquiryAck(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}

	/// <summary>FIX 4.4 as this package compiles it, for <see cref="FixMessage.ConfirmationRequest"/>.</summary>
	/// <param name="message">The message to check.</param>
	/// <param name="findings">Where to put what is wrong.</param>
	public static void ValidateConfirmationRequest(FixMessage message, List<FixFinding> findings)
	{
		FixRules.Check(CompiledTables.Instance, message, findings);
	}
}
