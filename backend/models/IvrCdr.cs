using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyApi.Models;

[Table("IvrCdrs")]
public partial class IvrCdr
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    [StringLength(20)]
    public string? Version { get; set; }

    [Column("Server_Id")]
    [StringLength(50)]
    public string? ServerId { get; set; }

    [Column("Is_Incoming_Call")]
    [StringLength(1)]
    public string? IsIncomingCall { get; set; }

    [Column("Call_id")]
    [StringLength(100)]
    public string? CallId { get; set; }

    [Column("Parent_Call_Id")]
    [StringLength(100)]
    public string? ParentCallId { get; set; }

    [Column("Sip_From")]
    [StringLength(200)]
    public string? SipFrom { get; set; }

    [Column("Sip_P_Asserted_Id")]
    [StringLength(200)]
    public string? SipPAssertedId { get; set; }

    [Column("Received_Calling_Number")]
    [StringLength(50)]
    public string? ReceivedCallingNumber { get; set; }

    [Column("Processed_Calling_Number")]
    [StringLength(50)]
    public string? ProcessedCallingNumber { get; set; }

    [Column("Received_Called_Number")]
    [StringLength(50)]
    public string? ReceivedCalledNumber { get; set; }

    [Column("Processed_Called_Number")]
    [StringLength(50)]
    public string? ProcessedCalledNumber { get; set; }

    [Column("Call_Start")]
    [MaxLength(6)]
    public DateTime? CallStart { get; set; }

    [Column("Ok_Time")]
    [MaxLength(6)]
    public DateTime? OkTime { get; set; }

    [Column("Call_End", TypeName = "datetime")]
    public DateTime? CallEnd { get; set; }

    [Column("Established_Duration")]
    public int? EstablishedDuration { get; set; }

    [Column("Total_Duration")]
    public int? TotalDuration { get; set; }

    [Column("Call_End_Reason")]
    [StringLength(20)]
    public string? CallEndReason { get; set; }

    [Column("Customer_Account_Id")]
    [StringLength(20)]
    public string? CustomerAccountId { get; set; }

    [Column("Customer_Billing_Id")]
    [StringLength(50)]
    public string? CustomerBillingId { get; set; }

    [Column("Reseller_Account_Id")]
    [StringLength(20)]
    public string? ResellerAccountId { get; set; }

    [Column("Reseller_Billing_Id")]
    [StringLength(50)]
    public string? ResellerBillingId { get; set; }

    [Column("DTMFS")]
    [StringLength(50)]
    public string? Dtmfs { get; set; }

    [Column("Trunk_Id")]
    [StringLength(20)]
    public string? TrunkId { get; set; }

    [StringLength(200)]
    public string? Prompt { get; set; }

    [Column("Tree_Id")]
    [StringLength(20)]
    public string? TreeId { get; set; }

    [Column("Sensor_Id")]
    [StringLength(50)]
    public string? SensorId { get; set; }
}
