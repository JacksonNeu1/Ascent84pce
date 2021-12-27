
#include "includes\ti84pce.inc"

 .assume ADL=1
 .org userMem-2
 .db tExtTok,tAsm84CeCmp



;0E30000h + 0200h

	call _homeup
	call _ClrScrnFull

	call _RunIndicOff		; turn off run indicator
	di			
	
	
	;jp time_test_start
	
	ld	hl,mpLcdPalette

	ld	a,%00000000 ;GGGBBBBB
	ld	(hl),a
	ld  a,%00000000 ; GRRRRRGG
	inc hl
	ld	(hl),a
	inc hl;1yellow
	ld	a,%11100000 ;GGGBBBBB
	ld	(hl),a
	ld  a,%11111111 ; GRRRRRGG
	inc hl
	ld	(hl),a
	
	inc hl;2blue
	ld	a,%00011111 ;GGGBBBBB
	ld	(hl),a
	ld  a,%00000000 ; GRRRRRGG
	inc hl
	ld	(hl),a
	
	inc hl;3r
	ld	a,%00000000 ;GGGBBBBB
	ld	(hl),a
	ld  a,%01111100 ; GRRRRRGG
	inc hl
	ld	(hl),a
	
	inc hl;4g
	ld	a,%11100000 ;GGGBBBBB
	ld	(hl),a
	ld  a,%10000011 ; GRRRRRGG
	inc hl
	ld	(hl),a
	
	inc hl;5 white
	ld	a,%11111111 ;GGGBBBBB
	ld	(hl),a
	ld  a,%11111111 ; GRRRRRGG
	inc hl
	ld	(hl),a

	call _os_ClearVRAMLines	; set all of vram to index 255 (white)
	ld	a,lcdBpp4
	ld (mpLcdCtrl),a	
	

	ld hl,vRam + (160*5)
	ld (draw_bg_vram_addr),hl
	ld hl,$000080
	call draw_bg_line
	

	
	
	ei
	call _GetKey
	di

exit_prgm:
	call _ClrScrnFull
	ld	a,lcdBpp16
	ld	(mpLcdCtrl),a
	call _DrawStatusBar
	
	ei				; reset screen back to normal
	ret				; return to os
	

printHL:;=================REMOVE
	push hl
	call _os_ClearVRAMLines	; set all of vram to index 255 (white)
	ld	a,lcdBpp16
	ld (mpLcdCtrl),a
	pop hl
	call _DispHL
	ei
	call _GetKey
	di
	jp exit_prgm


test_addr:
	.dl 0

draw_buffer:
	.dl 0

	
#include "timeTesting.txt"
#include "drawSprite.txt"
#include "spriteData.txt"
#include "levelData.txt"
