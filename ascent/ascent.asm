
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
	
	
	;call fast_fg_sprite_set_flip
	;jp time_test_start
	
	
	
	ld hl,testSpriteCompressed
	ld de,vRam 
	call slow_sprite_decompress
	
	
	ld de,vRam +(160*40)
	ld hl,vRam
	call draw_slow_fg_sprite_full
	
	ei
	call _GetKey
	di
	
	
	
	
	
	
	ld hl,vRam + (160*5)
	ld (draw_bg_vram_addr),hl
	ld hl,$000080
	call draw_bg_line
	
	
	
	
	;ld de,vram +(160*10)
	;ld hl,testFastSprite+3
	;exx
	;ld b,8
	;ld hl,8
	;ld de,testFastSprite_t - 1
	;ld c,%10000000
	;call draw_fast_fg_sprite
	
	
	ld de,vram +(160*20)
	ld hl,test_fast_sprite
	ld a,1
	call draw_fast_sprite_top_cut
	
	
	ld de,vram +(160*40)
	ld hl,testSlowSprite
	call draw_slow_fg_sprite_full
	
	ld de,vram +(160*30)
	ld hl,testSlowSprite
	ld a,2
	call draw_slow_sprite_bottom_cut
	
	ld de,vram +(160*30) + 6
	ld hl,testSlowSprite
	ld a,2
	call draw_slow_sprite_top_cut
	
	ei
	call _GetKey
	di

exit_prgm:
	call _ClrScrnFull
	ld	a,lcdBpp16
	ld	(mpLcdCtrl),a
	call _DrawStatusBar
	
	ei				; reset screen back to normal
	ret			; return to os


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


prgmpause:
	ei
	call _GetKey
	di
	ret

test_addr:
	.dl 0

draw_buffer:
	.dl 0

#include "timeTesting.txt"
#include "drawSprite.txt"
#include "drawFGSprite.txt"
#include "spriteData.txt"
#include "levelData.txt"
#include "spriteDecompress.txt"