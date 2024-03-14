//  ! RESET
//  PAYLOAD[2]
//  F[5:Reset Z]=(ON)
//  DO[1]=OFF

//  LBL[1]

//  IF (F[5:Reset Z]=(ON)) THEN
//  OVERRIDE=80%
//  F[5:Reset Z]=(OFF)
//J PR[4:Photo pos] 100% FINE
//  PR[2:Pick High]=PR[99]
//  PR[5:Place pos High]=PR[97]
//  R[11:LBL Register]=5
//  JMP LBL[10]
//  LBL[5]
//J PR[3:Pick pos] 3.0sec CNT100
//J PR[4:Photo pos] 100% FINE ACC10
//  CALL -INST_INSTSENS_END(1,0,0,0,0)

//  OVERRIDE=90%
//  ENDIF

//  IF (F[3:RunCommand]=(ON) AND R[1:Goto Position]>0) THEN
//  ! Run Command when robot is
//  ! in position only
//  ! Mark not in position
//  F[1:In Position]=(OFF)

//  ! Mark robot running
//  F[3:RunCommand]=(OFF)

//  IF (R[1:Goto Position]=1) THEN
//  ! Go Down to take piece
//  R[11:LBL Register]=11
//  JMP LBL[10]
//  LBL[11]

//  F[1:In Position]=(ON)

//  ! Force go down one cm
//  PR[2,3:Pick High]=PR[2,3:Pick High]-R[3:Non safety down ]
//  DO[1]=ON
//J PR[2:Pick High] 100% FINE
//  PR[2,3:Pick High]=PR[2,3:Pick High]+R[3:Non safety down ]
//  PR[2,3:Pick High]=PR[2,3:Pick High]+10
//  JMP LBL[1]
//  ENDIF

//  IF (R[1:Goto Position]=2) THEN
//  ! Go Up
//  PR[2,3:Pick High]=PR[2,3:Pick High]+R[2:Load Force]
//J PR[2:Pick High] 1.5sec CNT100
//  PR[2,3:Pick High]=PR[2,3:Pick High]-R[2:Load Force]
//  CALL -INST_INSTSENS_END(1,0,0,0,0)
//  DO[1]=OFF
//J PR[3:Pick pos] 100% CNT100
//J PR[4:Photo pos] 100% FINE
//  F[1:In Position]=(ON)
//  JMP LBL[1]
//  ENDIF

//  IF (R[1:Goto Position]=3) THEN
//J PR[5:Place pos High] 100% FINE
//  CALL -INST_INSTSENS_START(1,0,0,0,0)
//  CALL -INST_TOUCHSKIP_CRX(1,PR[6:Place pos Low],30,10,10,0,0,0,0,0)
//  F[1:In Position]=(ON)
//  PR[5:Place pos High]=LPOS
//  PR[5,3:Place pos High]=PR[5,3:Place pos High]+10

//  JMP LBL[1]
//  ENDIF

//  IF (R[1:Goto Position]=4) THEN
//  CALL -INST_INSTSENS_END(1,0,0,0,0)
//J PR[4:Photo pos] 100% FINE

//  F[1:In Position]=(ON)
//  JMP LBL[1]
//  ENDIF

//  IF (R[1:Goto Position]=5) THEN
//J PR[8:Half way to EP] 100% CNT10
//J PR[7:GET EP] 30% FINE
//  F[1:In Position]=(ON)
//  JMP LBL[1]
//  ENDIF
//  ENDIF

//  JMP LBL[1]

//  LBL[10]

//J PR[3:Pick pos] 100% CNT100
//J PR[2:Pick High] 100% FINE
//  CALL -INST_INSTSENS_START(1,0,0,0,0)
//  CALL -INST_TOUCHSKIP_CRX(1,PR[1:Pick Low],30,10,R[2:Load Force],0,0,0,0,0)
//  PR[2:Pick High]=LPOS
//  PR[2,3:Pick High]=PR[2,3:Pick High]+20
//  PR[6,3:Place pos Low]=PR[2,3:Pick High]+10
//  JMP LBL[R[11]]  ! RESET
//  PAYLOAD[2]
//  F[5:Reset Z]=(ON)
//  DO[1]=OFF

//  LBL[1]

//  IF (F[5:Reset Z]=(ON)) THEN
//  OVERRIDE=80%
//  F[5:Reset Z]=(OFF)
//J PR[4:Photo pos] 100% FINE
//  PR[2:Pick High]=PR[99]
//  PR[5:Place pos High]=PR[97]
//  R[11:LBL Register]=5
//  JMP LBL[10]
//  LBL[5]
//J PR[3:Pick pos] 3.0sec CNT100
//J PR[4:Photo pos] 100% FINE ACC10
//  CALL -INST_INSTSENS_END(1,0,0,0,0)

//  OVERRIDE=90%
//  ENDIF

//  IF (F[3:RunCommand]=(ON) AND R[1:Goto Position]>0) THEN
//  ! Run Command when robot is
//  ! in position only
//  ! Mark not in position
//  F[1:In Position]=(OFF)

//  ! Mark robot running
//  F[3:RunCommand]=(OFF)

//  IF (R[1:Goto Position]=1) THEN
//  ! Go Down to take piece
//  R[11:LBL Register]=11
//  JMP LBL[10]
//  LBL[11]

//  F[1:In Position]=(ON)

//  ! Force go down one cm
//  PR[2,3:Pick High]=PR[2,3:Pick High]-R[3:Non safety down ]
//  DO[1]=ON
//J PR[2:Pick High] 100% FINE
//  PR[2,3:Pick High]=PR[2,3:Pick High]+R[3:Non safety down ]
//  PR[2,3:Pick High]=PR[2,3:Pick High]+10
//  JMP LBL[1]
//  ENDIF

//  IF (R[1:Goto Position]=2) THEN
//  ! Go Up
//  PR[2,3:Pick High]=PR[2,3:Pick High]+R[2:Load Force]
//J PR[2:Pick High] 1.5sec CNT100
//  PR[2,3:Pick High]=PR[2,3:Pick High]-R[2:Load Force]
//  CALL -INST_INSTSENS_END(1,0,0,0,0)
//  DO[1]=OFF
//J PR[3:Pick pos] 100% CNT100
//J PR[4:Photo pos] 100% FINE
//  F[1:In Position]=(ON)
//  JMP LBL[1]
//  ENDIF

//  IF (R[1:Goto Position]=3) THEN
//J PR[5:Place pos High] 100% FINE
//  CALL -INST_INSTSENS_START(1,0,0,0,0)
//  CALL -INST_TOUCHSKIP_CRX(1,PR[6:Place pos Low],30,10,10,0,0,0,0,0)
//  F[1:In Position]=(ON)
//  PR[5:Place pos High]=LPOS
//  PR[5,3:Place pos High]=PR[5,3:Place pos High]+10

//  JMP LBL[1]
//  ENDIF

//  IF (R[1:Goto Position]=4) THEN
//  CALL -INST_INSTSENS_END(1,0,0,0,0)
//J PR[4:Photo pos] 100% FINE

//  F[1:In Position]=(ON)
//  JMP LBL[1]
//  ENDIF

//  IF (R[1:Goto Position]=5) THEN
//J PR[8:Half way to EP] 100% CNT10
//J PR[7:GET EP] 30% FINE
//  F[1:In Position]=(ON)
//  JMP LBL[1]
//  ENDIF
//  ENDIF

//  JMP LBL[1]

//  LBL[10]

//J PR[3:Pick pos] 100% CNT100
//J PR[2:Pick High] 100% FINE
//  CALL -INST_INSTSENS_START(1,0,0,0,0)
//  CALL -INST_TOUCHSKIP_CRX(1,PR[1:Pick Low],30,10,R[2:Load Force],0,0,0,0,0)
//  PR[2:Pick High]=LPOS
//  PR[2,3:Pick High]=PR[2,3:Pick High]+20
//  PR[6,3:Place pos Low]=PR[2,3:Pick High]+10
//  JMP LBL[R[11]]