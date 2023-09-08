( chip 8 Assembler for [Volks-] Forth )
( 2023 by Carsten Strotmann, psc     )
( Command definitions from https://github.com/trapexit/chip-8_documentation )
( Licensed under GPL3 or later )

.( Chip 8 Assembler ) CR

forget marker
create marker
vocabulary ch8asm
onlyforth ch8asm also definitions

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
( Index Register )
$FF CONSTANT IR
( Sound Timer )
$F1 CONSTANT ST
( Delay Timer )
$F2 CONSTANT DT

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
  2 + $0FFF AND 'ORG !
  'ORG @ maxmem @ > IF 'ORG @ maxmem ! THEN
;

( CLS memnonic - clear screen )
: CLS $00E0 ORG! ;

( RET memnonic - return from subroutine )
: RET $00EE ORG! ;

( JMP memnonic - jump to address )
: JMP ( NNN - )
  $0FFF AND $1000 OR ORG! ;

( JSR Memnonic aka CALL - jump to subroutine at address )
: JSR ( NNN - )
  $0FFF AND $200 OR ORG! ;

( Skip if equal aka SE )
: SEQ ( vx NN - )
  $00FF AND $3000 OR
  SWAP $0F AND >< OR ORG! ;

( Skip if not equal )
: SNE ( vx NN -- )
  $00FF AND $4000 OR
  SWAP $0F AND >< OR ORG! ;

( Skip if Register equal aka SE VX,VY)
: SRE ( vy vx -- )
  $00FF AND >< $5000 OR SWAP
  $00FF AND 4 LSHIFT OR ORG! ;

( Load immediate value into register )
: LDV ( vx NN -- )
  $00FF AND SWAP $00FF AND >< OR
  $6000 OR ORG! ;

( Add immediate value to register )
: ADV ( vx NN -- )
  $00FF AND SWAP $00FF AND >< OR
  $7000 OR ORG! ;

( Adjust parameter for all of the commands starting with $8 )
 : (8cmd) ( vx vy -- )
  $00FF AND >< SWAP $00FF AND
  4 lshift OR ;

( Copy register vy into vx )
: CPR ( vy vx -- )
  (8cmd) $8000 OR ORG! ;

( Set VX equal to VX plus VY. In the case of an overflow )
( VF is set to 1. Otherwise 0. )
: ADD ( vy vx -- )
  (8cmd) $8004 OR ORG! ;

( Set VX equal to VX minus VY. In the case of an underflow )
( VF is set 0. Otherwise 1.  VF = VX > VY )
: SUB ( vy vx -- )
  (8cmd) $8005 OR ORG! ;

( Set VX equal to VX bitshifted right 1. VF is set to the least )
( significant bit of VX prior to the shift. Originally this opcode )
( meant set VX equal to VY bitshifted right 1 but emulators and software )
( seem to ignore VY now. )
: SHRV ( vy vx -- )
  (8cmd) $8006 OR ORG! ;
: SHR ( vx -- )
  V0 SWAP SHRV ;

( Set VX equal to VY minus VX. VF is set to 1 if VY > VX. Otherwise 0 )
: SBR ( vy vx -- )
  (8cmd) $8007 OR ORG! ;

( Set VX equal to VX bitshifted left 1. VF is set to the most significant )
( bit of VX prior to the shift. Originally this opcode meant set VX equal )
( to VY bitshifted left 1 but emulators and software seem to ignore VY now )
: SHLV ( vy vx -- )
  (8cmd) $800E OR ORG! ;
: SHL ( vx -- )
  V0 SWAP SHLV ;

( Skip the next instruction if VX does not equal VY )
: SNEV ( vy vx -- )
  (8cmd) $9000 OR ORG! ;

( Set I equal to NNN )
: LDI ( NNN -- )
  $0FFF AND $A000 OR ORG! ;

( Set the PC to NNN plus the value in V0 )
: JPV ( NNN -- )
  $0FFF AND $B000 OR ORG! ;

( Set VX equal to a random number ranging from 0 to 255 )
(  which is logically anded with NN )
: RND ( NN vx -- )
  $00FF AND >< OR $C000 OR ORG! ;

( Display N-byte sprite starting at memory location I at VX, VY. )
( Each set bit of xored with what's already drawn. VF is set to  )
( 1 if a collision occurs. 0 otherwise. )
: DRW ( n vy vx -- )
  (8cmd) SWAP $000F AND OR
  $D000 OR ORG! ;

( Set VX equal to the bitwise or of the values in VX and VY )
: ORR ( vy vx -- )
  (8cmd) $8001 OR ORG! ;

( Set VX equal to the bitwise and of the values in VX and VY )
: AND ( vy vx -- )
  (8cmd) $8002 OR ORG! ;

( Set VX equal to the bitwise xor of the values in VX and VY )
: XOR ( vy vx -- )
  (8cmd) $8003 OR ORG! ;

( create label for jumps )
: label:
  CREATE 'org @ ,
  DOES> @ ;

( save binary image )
: savebin ( <name> )
  ch8mem maxmem @ savefile ;
