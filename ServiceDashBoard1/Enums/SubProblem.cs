namespace ServiceDashBoard1.Enums
{
    // These enums represent subproblem categories associated with each MainProblem.
    //  They are used in the ComplaintRegistrationsController to dynamically display relevant subproblems
    //    in the UI based on the selected main problem type.
    // Each enum value acts as an identifier to store and retrieve the specific subproblem selected by the user.
    // This approach helps keep the structure clean, strongly typed, and easy to maintain.

    public enum TrainingSubproblem
    {
        APPLICATION = 1,
        OPERATIONAL = 2,
        SOFTWARE = 3,
        NESTING = 4,
        TRAINING_OTHERS_ISSUES = 5

    }
    public enum MachineSubproblem
    {
        DRIVEALARM = 11,
        SHARD_LIMIT_ALARM= 12,
        ZIG_ZAG_CUTTING = 13,
        CAPACITANCE_SENSOR = 14,
        AXIS_SENSOR = 15,
        OTION_NOISE = 16,
        GAS_PRESSURE_ALARM = 17,
         AXIS_SODT_LIMIT = 18,
          GAS_LEAKAGE = 19,
         GAS_FLOW = 20,
          DIAGONAL= 111,
           COMMUNICATION_BREAK = 112,
            OVALITY_CIRCULARITY = 113,
             EPR_ISSUE = 114,
        MACHINE_OTHERS_ISSUES= 115



    }

    public enum PelletMachineSubproblem
    {
        NO_PALLET_MOVEMENT = 21,
        PALLET_A_B_LIMIT_ALARM = 22,
        PALLET_SPEED_PROBLEM = 23,
        FPALLET_SLOW_LIMIT_NOT_WORKING = 24,
        PALLET_CRASHING_WITH_LOCKING_SYSTEM_STOPPER = 25,
        PALLET_MAKING_NOISE_DURING_MOVEMENT = 26,
        PPALLET_CHAIN_BROKEN = 27,
        PALLET_CLAMPING_ISSUE = 28,
        CHAIN_GUIDE_WEAR_OUT_DAMAGE = 29,
        PALLET_MOTOR_MAKING_NOICE = 30,
        VFD_SHOWING_ERROR = 212,
        PALLET_RAIL_AND_WHEELS = 213,
        PELLET_MACHINE_OTHERS_ISSUES = 214
    }

    public enum LaserSubproblem
    {
        CRITICAL_ERROR = 31,
        FIBER_INTERLOCK = 32,
        PS_ALARM = 33,
        LOW_TEMPERATURE_ALARM = 34,
        HIGH_TEMPERATUE_LARM = 35,
        OVERHEAT = 36,
        BACK_REFLECTION = 37,
        AIMING_BEAM_ALARM = 38,
        LASER_OTHERS_ISSUES= 39




    }

    public enum ChillerSubproblem
    {
        LOW_TEMPERATURE = 41,
        WATER_LEAKAGE = 42,
        WATER_PUMP_CREATING_LOUD_NOICE = 43,
        FAN_NOT_WORKING = 44,
        COMPRESSOR_NOT_WORKING = 45,
        WATER_LEVEL_ALARM = 46,
        COMPRESSOR_MAKING_LOUD_NOICE = 47,
        BIG_TANK_WATER_PUMP_NOT_WORKING = 48,
        S_BIG_TANK_WATER_PUMP_NOT_WORKING = 49,
        CHILLER_OTHERS_ISSUES = 50

    }

    public enum ExhaustSuctionSubproblem
    {
        BLOWER_FUME_EXTRACTOR_MCB_TRIPPING = 51,
        SMOKE_FROM_BLOWER_MOTOR = 52,
        PLC_NOT_TURNING_ON_FUME_EXTRACTOR = 53,
        SOLENOID_VALVE_NOT_OPERATING_PURGE_STORE_CYCLE_ISSUE = 54,
        AUTP_FILTER_CLEANING = 55,
        NO_SUCTION_ON_BOTH_PALLET = 56,
        FLAPS_NOT_OPERATING = 57,
        MULTIPLE_FLAPS_OPENING_SAME_TIME = 58,
        EXHAUST_SUCTION_OTHERS_ISSUES = 59
    }

    public enum NestingSoftwareSubproblem
    {
        COMMON_CUT = 61,
        SIGMANEST_SOFTWARE_INSTALLATION = 62,
        E_CAT_SOFTWARE_INSTALLATION = 63,
        POST_ERROR = 64,
        NC_FILE_NOT_GENERATING = 65,
        DXF_DWG_FILE_IMPORT = 66,
        REPORT_GENERATION = 67,
        NC_FILE_WRONG_STRUCTURE = 68,
        NESTING_SOFTWARE_OTHERS_ISSUES = 69
    }

    public enum CuttingAppSubproblem
    {

        CUTTING_APP_OTHERS_ISSUES = 71
    }

    public enum CuttingHeadSubproblem
    {
        OPTICAL = 81,
        FREQLENT_PROTECTION_GAS_DAMAGE = 82,
        CERAMIC_BRAKING = 83,
        TRA_PLATE_DAMAGE = 84,
        HEAD_SENSING = 85,
        HEAD_TIP_TOUCH = 86,
        HEAD_TEMPERATURE_RISE, FALL = 87,
        HEAD_TEMPERATURE_ALARM = 88,
        HEAD_CALIBRATION = 89,
        CUTTING_HEAD_OTHERS_ISSUES = 90
    }

    public enum SoftwareSubproblem
    {
        SOFTWARE_NOT_BOOTING_UP = 91,
        LICENSE_EXPIRE = 92,
        PLC_NOT_RUNNING = 93,
        HMI_PARAMETER_GOT_CHANGED = 94,
        GHOST_INSTALL = 95,
        HMI_MALFUNCTION = 96,
        HMI_GOT_STUCK = 97,
        CUTTING_LAYER_NOT_WORKING = 98,
        HMI_LANGUAGE_GOT_CHANGED = 99,
        UNWANTED_ALARMS_GENERATED = 911,
        NO_LOADING_PARAMETERS = 912,
        MATERIAL_BURNING = 913,
        FLIM_CUT = 914,
        SOFTWARE_OTHERS_ISSUES = 915
    }

    public enum OtherIssuesSubproblem
    {
        OTHER_ISSUES = 1101

    }
}

