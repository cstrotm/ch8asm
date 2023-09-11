: SAVEFILE ( addr n / <name> -- )
  FILE" W/O OPEN-FILE DROP >R
  R@ WRITE-FILE DROP
  R> CLOSE-FILE ;
