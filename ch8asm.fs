( chip 8 Assembler for [Volks-] Forth )
( 2023 by Carsten Strotmann, psc     )
( Command definitions from https://github.com/trapexit/chip-8_documentation )
( Licensed under GPL3 or later )

.( Chip 8 Assembler ) CR

hex
forget marker
create marker
vocabulary ch8asm
ch8asm also definitions

( Chip 8 register definitions )
$00 CONSTANT V0
$01 CONSTANT V1
$02 CONSTANT V2
$03 CONSTANT V3
$04 CONSTANT V4
$05 CONSTANT V5
$06 CONSTANT V6
$07 CONSTANT V7
$08 CONSTANT V8
$09 CONSTANT V9
$0A CONSTANT VA
$0B CONSTANT VB
$0C CONSTANT VC
$0D CONSTANT VD
$0E CONSTANT VE
$0F CONSTANT VF

( 4KB Chip8 core memory)
create ch8mem $1000 allot
ch8mem $1000 erase

( highest memory address used )
VARIABLE maxmem 0 maxmem !

( Alias definitions for Forth "AND", "OR" and "XOR" )
' AND ALIAS $AND
' XOR ALIAS $XOR
' OR  ALIAS $OR

: Dump ( adr n -- )
  SWAP $0FFF AND SWAP
  OVER + SWAP
  DO I ch8mem + C@ . LOOP ;

( lshift - Perform a logical left shift of u bit-places on x1, giving x2. )
(  Put zeroes into the least significant bits vacated by the shift. )
(  An ambiguous condition exists if u is greater than or equal to the )
(  number of bits in a cell. )
: lshift ( x1 u -- x2 )
  0 ?DO 2* LOOP ;

( rshift - Perform a logical right shift of u bit-places on x1, giving x2. )
( Put zeroes into the most significant bits vacated by the shift. An )
( ambiguous condition exists if u is greater than or equal to the number )
( of bits in a cell. )
: rshift ( x1 u -- x2 )
  0 ?DO 2/ LOOP ;

( Byteswap )
: ><  ( 16b1 -- 16b2 )
  dup 8 lshift swap 8 rshift OR ;

( Assembly address )
VARIABLE 'ORG

( Set the assemble address )
: ORG ( n -- ) 'ORG ! ;

$200 ORG

( Store memnonic in current org memory address )
: ORG! ( n -- )
  'ORG @ ch8mem + !
  'ORG @ 2 + $0FFF AND 'ORG !
  'ORG @ maxmem @ > IF 'ORG @ maxmem ! THEN
;

: OORG! ( n mask -- )
  OR ORG! ;

( CLS memnonic - clear screen )
: CLS $00E0 ORG! ;

( RET memnonic - return from subroutine )
: RET $00EE ORG! ;

( JMP memnonic - jump to address )
: JMP ( NNN - )
  $0FFF AND $1000 OORG! ;

( JSR Memnonic aka CALL - jump to subroutine at address )
: JSR ( NNN - )
  $0FFF AND $200 OORG! ;

( Skip if equal aka SE )
: SEQ ( vx NN - )
  $00FF AND $3000 OR
  SWAP $0F AND >< OORG! ;

( Skip if not equal )
: SNE ( vx NN -- )
  $00FF AND $4000 OR
  SWAP $0F AND >< OORG! ;

( Skip if Register equal aka SE VX,VY)
: SRE ( vy vx -- )
  $00FF AND >< $5000 OR SWAP
  $00FF AND 4 LSHIFT OORG! ;

( Load immediate value into register )
: LDV ( vx NN -- )
  $00FF AND SWAP $00FF AND >< OR
  $6000 OORG! ;

( Add immediate value to register )
: ADV ( vx NN -- )
  $00FF AND SWAP $00FF AND >< OR
  $7000 OORG! ;

( Adjust parameter for all of the commands starting with $8 )
 : (8cmd) ( vx vy -- )
  $00FF AND >< SWAP $00FF AND
  4 lshift OR ;

( Copy register vy into vx )
: CPR ( vy vx -- )
  (8cmd) $8000 OORG! ;

( Set VX equal to VX plus VY. In the case of an overflow )
( VF is set to 1. Otherwise 0. )
: ADD ( vy vx -- )
  (8cmd) $8004 OORG! ;

( Set VX equal to VX minus VY. In the case of an underflow )
( VF is set 0. Otherwise 1.  VF = VX > VY )
: SUB ( vy vx -- )
  (8cmd) $8005 OORG! ;

( Set VX equal to VX bitshifted right 1. VF is set to the least )
( significant bit of VX prior to the shift. Originally this opcode )
( meant set VX equal to VY bitshifted right 1 but emulators and software )
( seem to ignore VY now. )
: SHRV ( vy vx -- )
  (8cmd) $8006 OORG! ;
: SHR ( vx -- )
  V0 SWAP SHRV ;

( Set VX equal to VY minus VX. VF is set to 1 if VY > VX. Otherwise 0 )
: SBR ( vy vx -- )
  (8cmd) $8007 OORG! ;

( Set VX equal to VX bitshifted left 1. VF is set to the most significant )
( bit of VX prior to the shift. Originally this opcode meant set VX equal )
( to VY bitshifted left 1 but emulators and software seem to ignore VY now )
: SHLV ( vy vx -- )
  (8cmd) $800E OORG! ;
: SHL ( vx -- )
  V0 SWAP SHLV ;

( Skip the next instruction if VX does not equal VY )
: SNEV ( vy vx -- )
  (8cmd) $9000 OORG! ;

( Set I equal to NNN )
: LDI ( NNN -- )
  $0FFF AND $A000 OORG! ;

( Set the PC to NNN plus the value in V0 )
: JPV ( NNN -- )
  $0FFF AND $B000 OORG! ;

( Set VX equal to a random number ranging from 0 to 255 )
(  which is logically anded with NN )
: RND ( NN vx -- )
  $00FF AND >< OR $C000 OORG! ;

( Display N-byte sprite starting at memory location I at VX, VY. )
( Each set bit of xored with what's already drawn. VF is set to  )
( 1 if a collision occurs. 0 otherwise. )
: DRW ( n vy vx -- )
  (8cmd) SWAP $000F AND OR
  $D000 OORG! ;

( Prepare for E/F commands using VX as parameter )
: (vxcmd)
  $000F AND >< ;

( Skip the following instruction if the key represented by the )
(  value in VX is pressed.)
: SKP ( vx -- )
  (vxcmd) $E09E OORG! ;

( Set VX equal to the delay timer. )
: LDT ( vx -- )
  (vxcmd) $F007 OORG! ;

( Wait for a key press and store the value of the key into VX. )
: LKY ( vx -- )
  (vxcmd) $F00A OORG! ;

( Set the delay timer DT to VX. )
: SDT ( vx -- )
  (vxcmd) $F015 OORG! ;

( Set the sound timer ST to VX. )
: SST ( vx -- )
  (vxcmd) $F018 OORG! ;

( Add VX to I. VF is set to 1 if I > 0x0FFF. Otherwise set to 0. )
: AVI ( vx -- )
  (vxcmd) $F01E OORG! ;

( Set I to the address of the CHIP-8 8x5 font sprite representing )
( the value in VX. )
: LDF ( vx -- )
  (vxcmd) $F029 OORG! ;

( Convert that word to BCD and store the 3 digits at memory location )
( I through I+2. I does not change. )
: BCD
  (vxcmd) $F033 OORG! ;

( Store registers V0 through VX in memory starting at location I. )
( I does not change )
: STR ( vx -- )
  (vxcmd) $F055 OORG! ;

( Copy values from memory location I through I + X into registers )
( V0 through VX. I does not change. )
: LDR ( vx -- )
  (vxcmd) $F065 OORG! ;

( Set VX equal to the bitwise or of the values in VX and VY )
: ORR ( vy vx -- )
  (8cmd) $8001 OORG! ;

( Set VX equal to the bitwise and of the values in VX and VY )
: AND ( vy vx -- )
  (8cmd) $8002 OORG! ;

( Set VX equal to the bitwise xor of the values in VX and VY )
: XOR ( vy vx -- )
  (8cmd) $8003 OORG! ;

( store singe bytes in memory )
: DB ( n -- )
  'ORG @ ch8mem + C!
  'ORG @ 1 + $0FFF AND 'ORG !
  'ORG @ maxmem @ > IF 'ORG @ maxmem ! THEN

( create label for jumps )
: label:
  CREATE 'ORG @ ,
  DOES> @ ;

( save binary image )
: savebin ( <name> )
  ." Saving binary ..." CR
  ch8mem maxmem @ savefile
  maxmem @ . ." bytes saved." CR ;
