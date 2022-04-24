
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
	
	ld hl,0
	
	ld a,($F20000)
	ld l,a
	ld a,($F20001);128hz 
	ld h,a
	push hl
	ld bc,(longestFrame)
	sbc hl,bc 
	jp c,longest_frame_skip
	
	pop hl 
	push hl 
	ld (longestFrame),hl 
	ld hl,(cam_pos)
	ld (longestFramePos),hl
longest_frame_skip:
	pop hl 
	
		
	ld hl,(frameCount)
	inc hl
	ld (frameCount),hl
	
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


	ld hl,0
	
	ld a,($F20002)
	ld h,a
	ld a,($F20001)
	ld l,a
	ld bc,(totalTime)
	add hl,bc 
	ld (totalTime),hl 
	
	
	ld hl,(cam_pos)
	inc hl
	inc hl
	inc hl
	inc hl
	inc hl
	inc hl
	inc hl
	inc hl
	ld (cam_pos),hl
	ld bc,239
	add hl,bc 
	ld a,h ;msb 
	cp 35
	jp nz,main_loop
		
	;print debug times 
	ld hl,vRam
	ld (mpLcdBase),hl
	call _os_ClearVRAMLines	; set all of vram to index 255 (white)
	ld	a,lcdBpp16
	ld (mpLcdCtrl),a
	
	ld a,0
	ld (curRow),a
	ld (curCol),a
	ld hl,(longestFrame)
	call _DispHL
	ld a,1
	ld (curRow),a
	ld a,0
	ld (curCol),a
	ld hl,(longestFramePos)
	call _DispHL
	ld a,2
	ld (curRow),a
	ld a,0
	ld (curCol),a
	ld hl,(totalTime)
	call _DispHL
	ld a,3
	ld (curRow),a
	ld a,0
	ld (curCol),a
	ld hl,(frameCount)
	call _DispHL
	
	call prgmpause
	call prgmpause
	
	
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

cam_pos:;bottom of cam
	.dl 0
bg_cam_pos:
	.dl 0


draw_buffer:
	.dl 0
BG_draw_buffer:;uppermost line of bg in vram
	.dl 0
BG_buffer .equ vram + (160*240)


longestFrame:
	.dl 0
longestFramePos:
	.dl 0
totalTime:
	.dl 0
frameCount:
	.dl 0
hasLagged:
	.dl 0





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


