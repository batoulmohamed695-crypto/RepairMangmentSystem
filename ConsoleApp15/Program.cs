using System;
using System.Collections.Generic;

// الطلب الخامس: فئة ثابتة للتحقق من البيانات + خاصية ثابتة لعدم التكرار وتتبع العدد
public static class ValidationHelper
{
    public static bool IsValidLength(string serialNumber)
    {
        return !string.IsNullOrEmpty(serialNumber) && serialNumber.Length >= 3;
    }
    //  (حساب أو تتبع عدد الأجهزة التي تمت معالجتها)
    public static int TotalProcessedDevices { get; set; } = 0;

    public static bool ValidateSerialNumber(string serialNumber)
    {
        //فحص بسيط للتأكد من أن الرقم ليس فارغاً 
        if (string.IsNullOrWhiteSpace(serialNumber))
            return false;

        if (!serialNumber.Contains("-"))
            return false;

        return true;
    }
}

public enum RepairStatus
{
    Pending,
    InProgress,
    Completed
}

//  ِ (Delegate) لإشعار تغير الحالة
public delegate void RepairStatusChangedHandler(string serialNumber, RepairStatus newStatus);

public interface IRepairable
{
    string SerialNumber { get; }
    RepairStatus Status { get; }

    decimal CalculateRepairCost();
    void ChangeStatus(RepairStatus newStatus);
    string GetSummary();
}

// الفئة المجردة الأساسية للأجهزة
public abstract class Appliance : IRepairable
{
    private decimal partsCost;
    private double laborHours;
    private decimal laborRatePerHour;
    private RepairStatus status;

    // الحدث الخاص بتغير الحالة
    public static event RepairStatusChangedHandler OnStatusChanged;

    public string SerialNumber { get; private set; }

    public RepairStatus Status
    {
        get { return status; }
    }

    public abstract string ApplianceType { get; }
    public string GetStatusDescription()
{
    return "Current Repair State: " + status.ToString();
}
    protected Appliance(string serialNumber, decimal partsCost, double laborHours, decimal laborRatePerHour)
    {
        bool isValid = ValidationHelper.ValidateSerialNumber(serialNumber);
        if (!isValid)
        {
            throw new ArgumentException("Error: Serial number is not valid!");
        }

        SerialNumber = serialNumber;
        this.partsCost = partsCost;
        this.laborHours = laborHours;
        this.laborRatePerHour = laborRatePerHour;
        this.status = RepairStatus.Pending;
    }


    public abstract decimal CalculateRepairCost();

    // دالة لحساب التكلفة الأساسية المشتركة
    protected decimal CalculateBaseCost()
    {
        decimal baseTotal = partsCost + (decimal)laborHours * laborRatePerHour;
        return baseTotal;
    }

    public void ChangeStatus(RepairStatus newStatus)
    {
        status = newStatus;

       
        RepairStatusChangedHandler handler = OnStatusChanged;
        if (handler != null)
        {
            handler(SerialNumber, status);
        }
    }

    public string GetSummary()
    {
        return "Appliance Type: " + ApplianceType + ", Serial No: " + SerialNumber + ", Current Status: " + Status;
    }
}


public class Refrigerator : Appliance
{
    private bool hasFreezer;
public bool CheckFreezerAvailable()
{
    return hasFreezer;
}
    public override string ApplianceType
    {
        get { return "Refrigerator"; }
    }

    public bool HasFreezer
    {
        get { return hasFreezer; }
    }

    public Refrigerator(string serialNumber, decimal partsCost, double laborHours, decimal laborRatePerHour, bool hasFreezer)
        : base(serialNumber, partsCost, laborHours, laborRatePerHour)
    {
        this.hasFreezer = hasFreezer;
    }

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost();
        if (hasFreezer == true)
        {
            cost = cost + 50.0m; // تكلفة إضافية للثلاجة التي تحتوي على فريزر
        }
        return cost;
    }
}

// فئة الغسالة
public class WashingMachine : Appliance
{
    private int capacityKg;

    public override string ApplianceType
    {
        get { return "WashingMachine"; }
    }

    public int CapacityKg
    {
        get { return capacityKg; }
    }
    public WashingMachine(string serialNumber, decimal partsCost, double laborHours, decimal laborRatePerHour, int capacityKg)
        : base(serialNumber, partsCost, laborHours, laborRatePerHour)
    {
        this.capacityKg = capacityKg;
    }

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost();
        if (capacityKg > 8)
        {
            cost = cost + 30.0m; // زيادة السعة الكبيرة
        }
        return cost;
    }
}

// فئة المكيف
public class AirConditioner : Appliance
{
    private double btu;

    public override string ApplianceType
    {
        get { return "AirConditioner"; }
    }

    public double Btu
    {
        get { return btu; }
    }

    public AirConditioner(string serialNumber, decimal partsCost, double laborHours, decimal laborRatePerHour, double btu)
        : base(serialNumber, partsCost, laborHours, laborRatePerHour)
    {
        this.btu = btu;
    }

    public override decimal CalculateRepairCost()
    {
        decimal cost = CalculateBaseCost();
        cost = cost + 40.0m; // تكلفة صيانة خاصة بالمكيفات
        return cost;
    }
}

public class Program
{
    //دالة معالجة الحدث ت Lambda 
    public static void DisplayAlertMessage(string serial, RepairStatus status)
    {
        Console.WriteLine("-> [Event Notification]: Device {0} status has changed to: {1}", serial, status);
    }

    public static void Main(string[] args)
    {
        // ربط الحدث بالطريقة التقليدية الواضحة للأساتذة
        Appliance.OnStatusChanged += new RepairStatusChangedHandler(DisplayAlertMessage);

        // إنشاء قائمة لتطبيق تعدد الأشكال (Polymorphism List)
        List<IRepairable> myWorkshopList = new List<IRepairable>();

        myWorkshopList.Add(new Refrigerator("REF-101", 120.0m, 2.5, 30.0m, true));
        myWorkshopList.Add(new WashingMachine("WM-202", 80.0m, 1.5, 25.0m, 9));
        myWorkshopList.Add(new AirConditioner("AC-303", 200.0m, 3.0, 35.0m, 18000));

        // حلقة تكرار لعرض البيانات وحساب التكاليف لكل الأجهزة
        foreach (IRepairable item in myWorkshopList)
        {
            Console.WriteLine(item.GetSummary());

            decimal totalCost = item.CalculateRepairCost();
            Console.WriteLine("Total Repair Cost: " + totalCost);

            // تغيير الحالة لاختبار الحدث[
            item.ChangeStatus(RepairStatus.InProgress);
            item.ChangeStatus(RepairStatus.Completed);

            // زيادة عداد الفئة الثابتة عند الانتهاء من كل جهاز
            ValidationHelper.TotalProcessedDevices++;

            Console.WriteLine("---------------------------------------------");
        }

        // طباعة قيمة الخاصية الثابتة للتأكد من عملها
        Console.WriteLine("Total processed devices counted by static class: " + ValidationHelper.TotalProcessedDevices);

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
