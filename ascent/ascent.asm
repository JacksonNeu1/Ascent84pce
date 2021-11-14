
 .assume ADL=1
 .org userMem-2
 .db tExtTok,tAsm84CeCmp

draw_buffer:
	.dl 0
;0E30000h + 0200h

	call _homeup
	call _ClrScrnFull

	call _RunIndicOff		; turn off run indicator
	di				; disable interrupts
	ld	hl,mpLcdPalette

	ld	a,%00000000 ;GGGBBBBB
	ld	(hl),a
	ld  a,%00000000 ; GRRRRRGG
	inc hl
	ld	(hl),a
	inc hl
	ld	a,%11100000 ;GGGBBBBB
	ld	(hl),a
	ld  a,%11111111 ; GRRRRRGG
	inc hl
	ld	(hl),a

	call _os_ClearVRAMLines	; set all of vram to index 255 (white)
	ld	a,lcdBpp4
	ld	(mpLcdCtrl),a	


	ld hl,vRam
	ld (draw_buffer),hl
	ld bc,320*120
	ld a,$11
	call _MemSet
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
	
	
#include "includes\ti84pce.inc"
#include "drawSprite.txt"
#include "spriteData.txt"
#include "levelData.txt"
