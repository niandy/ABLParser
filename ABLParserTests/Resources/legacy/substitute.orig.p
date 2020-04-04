DEFINE VARIABLE numWidgets AS INTEGER NO-UNDO INITIAL 42.
DEFINE VARIABLE i-�bernahme AS INTEGER NO-UNDO INITIAL 12.

&GLOBAL-DEFINE yahoo " widgets remaining"

/* Preprocessing prevents auto-refactor */
DISPLAY "You have " + STRING(numWidgets) + {&yahoo}.



/* Mixed string attributes prevents auto-refactor */
DISPLAY "You have ":L + STRING(numWidgets) + " widgets remaining":R.



/* Mixed quotation types prevents auto-refactor */
DISPLAY "You have " + STRING(numWidgets) + ' "widgets" remaining'.



/* No need to refactor - less than two translatable strings */
DISPLAY "You have " + STRING(numWidgets) + " widgets remaining":U.



/* Auto-refactor OK - demonstrates mixed translatable/untranslatable */
DISPLAY "You have " + STRING(numWidgets) + " " + "widgets":U + " remaining".



/* Test umlaut (extended) characters like � */
DISPLAY "�bernahme 1 " + STRING(i-�bernahme) + " 2 �bernahme".



/* Auto-refactor OK - The simplest case */
DISPLAY "You have " + STRING(numWidgets) + " widgets remaining".


