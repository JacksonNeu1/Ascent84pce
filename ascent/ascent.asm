
#include "includes\ti84pce.inc"

 .assume ADL=1
 .org userMem-2
 .db tExtTok,tAsm84CeCmp



;0E30000h + 0200h

	call _homeup
	call _ClrScrnFull

	call _RunIndicOff		; turn off run indicator
	di			
	
	
	call setup_palette_1

	call _os_ClearVRAMLines	; set all of vram to index 255
	ld	a,lcdBpp4
	ld (mpLcdCtrl),a	

	

	call decompress_calls
	
	;gen purp timers
		; 76543210	
	ld a,%00000011;enable, 32768hz
	ld ($F20030),a
	;	  fedcba98
	ld a,%00000010;count up
	ld ($F20031),a
	
	ld hl,vRam + (160*240*2)
	ld (draw_buffer),hl
	ld hl,vRam + (160*240*3)
	ld (mpLcdBase),hl
	
	ld hl,0
	ld (cam_pos),hl

	call setup_bg

	;call draw_bg
		
	
main_loop:
	;clear timer
	ld a,0
	ld ($F20000),a;32768hz
	ld ($F20001),a;128hz
	ld ($F20002),a;0.5hz
	ld ($F20003),a
	
	;Instructions here 
	call draw_bg
	
	call draw_mg
	
	call draw_fg
	
	
	;swap draw buffers
	ld hl,(mpLcdBase)
	ld de,(draw_buffer)
	ld (mpLcdBase),de
	ld (draw_buffer),hl
	
	;check if lcd has drawn first frame
clock_check_loop:
	ld a,($F20001);128hz clock
	cp 3;check if reached 3 
	jp c,clock_check_loop

	;wait until finished drawing second frame

clear_int:      
    ld hl, mpLcdIcr
    set 2, (hl)            ; clear interrupt
    ld hl, mpLcdRis
wait_int:      
    bit 2, (hl)
    jr z, wait_int  

	
	
	ld hl,(cam_pos)
	inc hl
	inc hl
	ld (cam_pos),hl
	ld bc,239
	add hl,bc 
	ld a,h ;msb 
	cp 8
	jp z,exit_prgm
	
	
	;call prgmpause
	
	jp main_loop 
	
	
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

cam_pos:
	.dl 0
bg_cam_pos:
	.dl 0


draw_buffer:
	.dl 0
BG_draw_buffer:;uppermost line of bg in vram
	.dl 0
BG_buffer .equ vram + (160*240)



#include "timeTesting.txt"
#include "drawBGSprite.txt"
#include "drawFGSprite.txt"
#include "spriteDecompress.txt"
#include "drawFG.txt"

#include "levelData.txt"
#include "FGLevelData.txt"

#include "Sprite_Data.txt"
#include "FG_Data.txt"
#include "MG_Data.txt"
#include "BG_Data.txt"
#include "Palette_Setup.txt"
#include "Equates.txt"
#include "Decompress_Calls.txt"
#include "Sprite_Tables.txt"


