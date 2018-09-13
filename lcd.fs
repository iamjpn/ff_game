\ nokia -  lcd.txt
\ Jordan Niethe
\ Words to drive the SPI nokia on the ATmega328P
-lcd
marker -lcd

$2a constant ddrd
$2b constant portd
1 constant s.up
2 constant s.down 
3 constant s.still 

\ arduino 7 -> pd7
\ arduino 6 -> pd6
\ arduino 5 -> pd5
\ arduino 9 -> pb1
\ bit masks

%000010 constant mLED ( PB1 )
%100000 constant mDC ( PD5 )
%1000000 constant mRST ( PD6 )
%10000000 constant mSCE ( PD7 )

#84 constant LCD_WIDTH
#48 constant LCD_HEIGHT 
#1 constant WHITE
#0 constant BLACK
create display.array LCD_WIDTH LCD_HEIGHT * #8 / allot

\ ! SS is on PB2

: enable.lcd
	mSCE portd mclr
;

: disable.lcd
	mSCE portd mset
;

: select.command
	mDC portd mclr
;


: select.data
	mDC portd mset
;

: send.command ( c1 -- )
	select.command
	enable.lcd
	spi.csend
	disable.lcd
;

: send.data ( c1 -- )
	select.data
	enable.lcd
	spi.csend
	disable.lcd
;

: lcd.goto ( x y -- )
	$40 or send.command
	$80 or send.command
;

: pixel.set ( x y -- )
	2dup ( x y x y )
	#8 / LCD_WIDTH * 
	+
	display.array + ( x y address )
	@ ( x y value )
	over ( x y value y )
	#8 mod ( x y value [ y % 8 ] )
	#1 ( x y value [ y % 8 ] 1 )
	swap ( x y value 1 [ y % 8 ] )
	lshift ( x y value [ 1 << [ y % 8 ] ] )
	or ( x y [ value or [ 1 << [ y % 8 ] ] ] )
	>r ( x y )
	#8 / LCD_WIDTH * 
	+
	display.array + ( x y address )
	r>
	swap
	!
;


: pixel.clear ( x y -- )
	2dup ( x y x y )
	#8 / LCD_WIDTH * 
	+
	display.array + ( x y address )
	@ ( x y value )
	over ( x y value y )
	#8 mod ( x y value [ y % 8 ] )
	#1 ( x y value [ y % 8 ] 1 )
	swap ( x y value 1 [ y % 8 ] )
	lshift ( x y value [ 1 << [ y % 8 ] ] )
	invert	
	and ( x y [ value or [ 1 << [ y % 8 ] ] ] )
	>r ( x y )
	#8 / LCD_WIDTH * 
	+
	display.array + ( x y address )
	r>
	swap
	!
;

: display.clear
	display.array LCD_WIDTH LCD_HEIGHT * #8 / erase
;

: lcd.update 
	0 0 lcd.goto
	LCD_WIDTH LCD_HEIGHT * #8 / 0 do
		i display.array + @ send.data
		\ 0 send.data
	loop
;

: square ( x y -- )
	create
	c,
	c,
	0 c,
	0 c,
	8 allot
;

: square.data ( square offset -- ad )
	+ 4 + 
;

: fill.square ( square -- )
	0 square.data 8 $ff fill
;

: empty.square ( square -- )
	0 square.data 8 0 fill
;

0 0 square blank.square
blank.square empty.square

: get.x ( square -- x )
	1 + c@
;

: get.y ( square -- x )
	0 + c@
;

: get.jump ( square -- x )
	2 + c@
;

: get.dir ( square -- x )
	3 + c@
;

: set.x ( val square --  )
	1 + c!
;

: set.jump ( val square --  )
	2 + c!
;

: set.dir ( val square --  )
	3 + c!
;

: set.y ( val square --  )
	0 + c!
;

: draw.square ( square -- )

	#8 0 do \ row   
		#8 0 do  ( column ) \ i inner, j outer 
			dup ( square square )
			j square.data c@ ( square data )
			1 i lshift ( square data mask )
			and  ( square flag )
			if ( square )
				\ it is set
				dup ( square square )
				get.x ( square x )
				i + ( square xpos )
				swap ( xpos square )
				dup ( xpos square square )
				get.y ( xpos square y )
				j + ( xpos square ypos )
				swap ( xpos ypos square )
				rot ( ypos square xpos )
				rot ( square xpos ypos )
				pixel.set ( square )
			else
				\ not set
				dup ( square square )
				get.x ( square x )
				i + ( square xpos )
				swap ( xpos square )
				dup ( xpos square square )
				get.y ( xpos square y )
				j + ( xpos square ypos )
				swap ( xpos ypos square )
				rot ( ypos square xpos )
				rot ( square xpos ypos )
				pixel.clear ( square )
			then
		loop
	loop
	drop
;



: collided-left? ( square enemy -- )
	get.x ( square enemy-lhs )
	#8 +
	swap ( enemy-lsh square )
	get.x ( enemy-lsh square-lhs )
	< 
	invert
;

: collided-right? ( square enemy -- )
	get.x ( square enemy.lsh )
	swap ( enemy.lsh square )
	get.x ( enemy.lhs square.lsh )
	#8 + ( enemy.lsh square.rhs )
	>
	invert
;

: collided-up? ( square enemy -- )
	get.y ( square enemy.lsh )
	swap ( enemy.lsh square )
	get.y ( enemy.lhs square.lsh )
	#8 + ( enemy.lsh square.rhs )
	>
	invert
;

: collided? ( square enemy )
	2dup ( square enemy square enemy )
	2dup ( square enemy square enemy square enemy )
	collided-left? ( s e square enemy l )
	>r ( s e square enemy )
	collided-right? ( s e r )
	r> ( s e r l )
	and
	>r
	collided-up?
	r>
	and
;

: move.square ( x-offset y-offset square -- )
	dup ( xof yof square square )
	get.x ( xof yof square x )
	over  ( xof yof square x square ) 
	get.y ( xof yof square x y )
	blank.square set.y
	blank.square set.x
	blank.square draw.square

	dup ( xof yof square square )
	get.x ( xof yof square x )
	over  ( xof yof square x square ) 
	get.y ( xof yof square x y )
	rot ( xof yof x y square )
	>r ( xof yof x y )
	rot ( xof x y yof )
	+ ( xof x sy )
	>r ( xof x )
	+ ( sx ) 
	r> ( sx sy )
	r> ( sx sy square )
	tuck  ( sx square sy square )
	set.y
	tuck
	set.x
	draw.square
;


: pos.square ( x-offset y-offset square -- )
	dup ( xof yof square square )
	get.x ( xof yof square x )
	over  ( xof yof square x square ) 
	get.y ( xof yof square x y )
	blank.square set.y
	blank.square set.x
	blank.square draw.square

	dup ( xof yof square square )
	rot ( xof square square yof )
	swap ( xof square yof square )
	set.y ( xof square )
	tuck ( square xof square )
	set.x
	draw.square
;

: left.square ( square  -- )
	dup ( square square )
	-2 0 rot move.square ( square )
	dup ( square square )
	get.x ( square x )
	#2 ( square x 0)
	< if 
		#75 0 rot move.square 
	else 
		drop
	then
;	

: enemy.left 
	dup left.square
	draw.square
;

: can.jump? ( square -- x )
	get.dir s.still = 
;

: start.jump ( square -- )
	s.up swap set.dir
;

: inc.jump ( square -- )
	dup ( square square )
	get.jump ( square jump )
	1 +  ( square newjump )
	swap set.jump
;

: dnc.jump ( square -- )
	dup ( square square )
	get.jump ( square jump )
	1 -  ( square newjump )
	swap set.jump
;

: update.square ( square -- )
	dup  ( square square )
	dup  ( square square square )
	get.dir ( square dir ) 
	s.up = if 
		dup inc.jump
		dup get.jump 8 = if
			dup s.down swap set.dir
			dup 0 swap set.jump
		then
		0 -2 rot 
		move.square
	else  
	dup  ( square square square )
	get.dir ( square dir ) 
	s.down = if 
		dup inc.jump
		dup get.jump 8 = if
			dup s.still swap set.dir
			dup 0 swap set.jump
		then
		0 2 rot 
		move.square
	else 
		draw.square	
	then then
	drop
;


: lcd.init ( -- )
	spi.init

	mSCE ddrd mset
	mSCE portd mset

	mRST ddrd mset
	mRST portd mclr
	mRST portd mset 

	mDC ddrd mset

	mLED ddrb mset
	mLED portb mset

	%00100001 send.command
	%10010000 send.command
	%00100000 send.command
	%00001100 send.command
;



