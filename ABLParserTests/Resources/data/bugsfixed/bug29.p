DEFINE TEMP-TABLE tt NO-UNDO
 FIELD fld1 AS CHAR.

MESSAGE "FOO".
FINALLY:
  FOR EACH tt:
    DISP tt.
  END.
END FINALLY.