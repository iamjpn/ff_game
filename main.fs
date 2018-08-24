\ main - main.fs
\ Jordan Niethe
\ Contains the main loop to play the game
-main
marker -main

1 constant s.up
2 constant s.down 
3 constant s.still 


\ create the player
#15 #30 square player 
s.still player set.dir
0 player set.jump
player fill.square
player draw.square

\ create an enemy 
#75 #30 square enemy 
enemy fill.square
enemy draw.square


: game.loop ( -- )
#15 #30 player pos.square
player fill.square

#75 #30 enemy pos.square
enemy fill.square
begin
right? player can.jump? and if s.up player set.dir then
enemy enemy.left
player update.square
lcd.update
10 ms
again
;


