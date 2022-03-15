
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
	call sdcomp_reset_bg_sprite
	call sdcomp_reset_fast_sprite
	call sdcomp_reset_flip
	call sdcomp_set_offset
	ld hl,testSpriteCompressed4
	ld de,vRam
	call sprite_decompress
	
	
	call sdcomp_set_flip
	call sdcomp_set_offset
	ld hl,testSpriteCompressed
	ld de,vRam+160
	call sprite_decompress
	
	
	
	ld de,vRam +(160*40)
	ld hl,vRam
	ld a,2
	call draw_slow_sprite_bottom_cut
	
	
	ld de,vRam +(160*30)
	ld hl,vRam
	call draw_slow_sprite_full
	
	
	ld de,vRam +(160*40)+5
	ld hl,vRam+160
	ld a,3
	call draw_slow_sprite_top_cut
	
	ld de,vRam +(160*30)+5
	ld hl,vRam+160
	call draw_slow_sprite_full
	
	
	
	
	
;	ld hl,vRam + (160*5)
;	ld (draw_bg_vram_addr),hl
;	ld hl,$000080
;	call draw_bg_line
	
	
	
	
	;ld de,vram +(160*10)
	;ld hl,testFastSprite+3
	;exx
	;ld b,8
	;ld hl,8
	;ld de,testFastSprite_t - 1
	;ld c,%10000000
	;call draw_fast_fg_sprite
	
	
;	ld de,vram +(160*20) - 1
;	ld hl,test_fast_sprite
;	ld a,1
;	call draw_fast_sprite_top_cut
	
	
	call sdcomp_reset_flip
	call sdcomp_set_fast_sprite
	ld hl,testSpriteCompressedFast
	ld de,vRam + (160*3)
	call sprite_decompress
		
	ld de,vRam +(160*70) - 1
	ld hl,vRam + (160*3)
	ld a,3
	call draw_fast_sprite_bottom_cut
	
	
	
	call sdcomp_set_flip
;	call sdcomp_set_fast_sprite
	ld hl,testSpriteCompressedFast
	ld de,vRam + (160*4)
	call sprite_decompress
	
		
	ld de,vRam +(160*70) + 5
	ld hl,vRam + (160*4)
	ld a,2
	call draw_fast_sprite_top_cut
	
	
	ld de,vRam +(160*79) + 5
	ld hl,vRam + (160*4)
	ld a,2
	call draw_fast_sprite_full
	
	
	ld hl,testBGSprite
	ld de,vRam + (160*90)
	ld a,0 
	call draw_bg_sprite_line
	
	ld hl,testBGSprite
	ld de,vRam + (160*91)
	ld a,1
	call draw_bg_sprite_line
	
	ld hl,testBGSprite
	ld de,vRam + (160*92)
	ld a,2 
	call draw_bg_sprite_line
	
	ld hl,testBGSprite
	ld de,vRam + (160*93)
	ld a,3 
	call draw_bg_sprite_line
	
	ld hl,testBGSprite
	ld de,vRam + (160*94)
	ld a,4 
	call draw_bg_sprite_line
	
	ld hl,testBGSprite
	ld de,vRam + (160*97)
	ld a,7 
	call draw_bg_sprite_line
	
	
	
	ld hl,vRam + (320*240)
	ld (mpLcdBase),hl
	
	call sdcomp_reset_fast_sprite
	call sdcomp_set_bg_sprite
	call sdcomp_reset_flip
	;call sdcomp_set_offset
	ld hl,testSpriteCompressed
	ld de,vRam+(160*6)
	call sprite_decompress
	
	
testBG1 .equ vRam+(160*240)
testBG2 .equ testBG1 + 160
testBG3 .equ testBG2 + 160
testBG4 .equ testBG3 + 160
	
	ld hl,testBGSpriteComp
	ld de,testBG1
	call sprite_decompress
	
	ld hl,testBGSpriteComp2
	ld de,testBG2
	call sprite_decompress
	
	ld hl,testBGSpriteComp3
	ld de,testBG3
	call sprite_decompress
	
	call sdcomp_reset_bg_sprite
	
	
	ld a,0
	ld hl,$d49600
	ld de,vRam + (320*240)
	call draw_bg_sprite_line
	
	
	ld a,255
	ld hl,vRam + (320*240)
	ld (dbgl_vram_line_start),hl
	ld hl,bg_data_frame_1
	call draw_bg_line 
	

	
	
	ld a,255
	ld hl,vRam + (320*240)
bg_draw_test_loop:
	ld (dbgl_vram_line_start),hl 
	ld bc,160
	add hl,bc 
	push hl
	push af
	ld hl,bg_data_frame_1
	call draw_bg_line 
	pop af
	pop hl
	dec a
	jp nz,bg_draw_test_loop

	call prgmpause
	
	ld hl,vRam
	ld (mpLcdBase),hl
	
	
	
	
	
#comment 	ld hl,vRam+(160*6)
	ld de,vRam + (160*90) + 10
	ld a,0 
	call draw_bg_sprite_line
	
	ld hl,vRam+(160*6)
	ld de,vRam + (160*91) + 10
	ld a,1 
	call draw_bg_sprite_line
	
	ld hl,vRam+(160*6)
	ld de,vRam + (160*92) + 10
	ld a,2 
	call draw_bg_sprite_line
	ld hl,vRam+(160*6)
	ld de,vRam + (160*93) + 10
	ld a,3 
	call draw_bg_sprite_line
	ld hl,vRam+(160*6)
	ld de,vRam + (160*94) + 10
	ld a,4
	call draw_bg_sprite_line
	ld hl,vRam+(160*6)
	ld de,vRam + (160*95) + 10
	ld a,5 
	call draw_bg_sprite_line
	ld hl,vRam+(160*6)
	ld de,vRam + (160*96) + 10
	ld a,6 
	call draw_bg_sprite_line
	ld hl,vRam+(160*6)
	ld de,vRam + (160*97) + 10
	ld a,7 
	call draw_bg_sprite_line
 #endcomment
	
	
	
	
	

exit_prgm:
	ld hl,vRam
	ld (mpLcdBase),hl
	call _ClrScrnFull
	ld	a,lcdBpp16
	ld	(mpLcdCtrl),a
	call _DrawStatusBar
	
	ei				; reset screen back to normal
	ret			; return to os


printHL:;=================REMOVE
	push hl
	ld hl,vRam
	ld (mpLcdBase),hl
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
	push de 
	push hl 
	ei
	call _GetKey
	di
	pop hl 
	pop de 
	ret

test_addr:
	.dl 0

draw_buffer:
	.dl 0

#include "timeTesting.txt"
#include "drawBGSprite.txt"
#include "drawFGSprite.txt"
#include "spriteData.txt"
#include "levelData.txt"
#include "spriteDecompress.txt"
