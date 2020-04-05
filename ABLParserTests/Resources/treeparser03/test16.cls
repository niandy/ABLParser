Class test16: 

    DEFINE VARIABLE hInstance As HANDLE NO-UNDO.

    // Variable picVariable Is visible in the root symbol scope !!!
    // Should Not be attached there...
    Function Dummy Return LOGICAL (INPUT picVariable As CHARACTER) In hInstance.

    METHOD PUBLIC VOID DoIt(INPUT picVariable AS CHARACTER):
        MESSAGE picVariable.
    END METHOD.

End Class.