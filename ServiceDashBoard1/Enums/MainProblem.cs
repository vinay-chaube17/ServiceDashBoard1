namespace ServiceDashBoard1.Enums
{
    // This enum defines the list of main problem categories.
    // It is used in the ComplaintRegistrationsController to show main problems in the UI.
    // Based on the selected main problem, related subproblems are dynamically displayed.
    // Each value has a corresponding ID used for storage and logic mapping.

    public enum MainProblem
    {
        TRAINING = 1,
        MACHINE = 2,
        PALLET_MACHINE = 3,
        CUTTING_HEAD = 4,
        SOFTWARE = 5,
        LASER = 6,
       CHILLER = 7,
        EXHAUST_SUCTION = 8,
        NESTING_SOFTWARE = 9,
        CUTTING_APP = 10,
       OTHER_ISSUES = 11
    }
}
