\ input.fs 
\ Used to read two GPIO on the Atmega 328p to 
\ correspond to moving left and right
\ Jordan Niethe
\ want arduino pin two to be input 

\ pin two = pd2
-input
marker -input

$2a constant ddrd
$29 constant pind 
$2b constant portd

%000100 constant LEFT_IN ( PB1 )
%001000 constant RIGHT_IN ( PB2 )

: input.init ( -- )
	LEFT_IN ddrd mclr 
	RIGHT_IN ddrd mclr 

;

: input.read ( value -- value )
	 pind mtst  
;

: right? ( -- value )
	RIGHT_IN input.read
;

: left? ( -- value )
	LEFT_IN input.read
;
