
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

	call setup_decompress_queue
	
	
	
	;call draw_fg
	
	ld a,0
	call cfdc_cam_move_up ;need to skip here for frame 0
	
	call prgmpause

	call continue_decompressions
	
	call continue_decompressions
	
	call continue_decompressions

	call continue_decompressions
	
	call prgmpause
	call continue_decompressions
	
	
;	call decompress_calls
	
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

	;call setup_bg

	
main_loop:
	;clear timer
	ld a,0
	ld ($F20000),a;32768hz
	ld ($F20001),a;128hz
	ld ($F20002),a;0.5hz
	ld ($F20003),a
	
	;Instructions here 
	;call draw_bg
	
	;TEST CLEAR BUFFER
	ld hl,BG_buffer
	ld de,(draw_buffer)
	ld bc, 160*240
	ldir
	
	;call draw_mg
	
	call draw_fg
	
	ld hl,0
	
	; for debug longest frame draw time
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

	call check_for_decompress_calls
	;Sprite decompression will occur here
	call continue_decompressions
	

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
	
	;Move cam
	ld hl,(cam_pos)
	inc hl

	ld (cam_pos),hl
	
	;Check for end of demo
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


write_a_to_ram:
	push af 
	push hl 
write_a_to_ram_addr .equ $ + 1 
	ld hl, $d43000
	ld (hl),a 
	inc hl 
	ld (write_a_to_ram_addr),hl 
	pop hl 
	pop af 
	ret 

prgmpause: ;for testing, interrupts code until key pressed. will destroy af register
	push af
	push de 
	push hl 
	ei
	call _GetKey
	di
	pop hl 
	pop de 
	pop af
	ret

cam_pos:;y position of lowest visible line in fg layer
	.dl 79
bg_cam_pos: ;y position of lowest visible line in bg layer (= cam pos / 4)
	.dl 0


draw_buffer:;where new frame is drawn before lcd pointer is swapped 
	.dl $d52c00
	
BG_draw_buffer: ;Address of the uppermost line of the background buffer. This is where new lines of bg are drawn to 
	.dl 0
BG_buffer .equ vram + (160*240) ;Start of BG buffer 


;d40000 = Decompressed sprite data
;d49600 = BG buffer
;d52c00 = Frame draw buffer 1 
;d5c200 = frame draw buffer 2

;pixelShadow .equ $D031F6 


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
#include "BetterSpriteDecompress.txt"
#include "drawFG.txt"
#include "SpriteDecompressManager.txt"
;#include "levelData.txt"
;#include "FGLevelData.txt"

#include "generated/BG_Data.txt"
#include "generated/MG_Data.txt"
#include "generated/FG_Data.txt"
#include "generated/DecompressCalls.txt"
#include "generated/Palette_Setup.txt"
#include "generated/Sprite_Tables.txt"
#include "generated/Sprite_Data.txt"
#include "generated/SpriteEquates.txt"


;
;#include "TestingSpriteData.txt"
;#include "TestGeneratedSpriteData.txt"
;#include "Sprite_Data.txt"
;#include "FG_Data.txt"
;#include "MG_Data.txt"
;#include "BG_Data.txt"
;#include "Palette_Setup.txt"
;#include "Equates.txt"
;#include "Decompress_Calls.txt"
;#include "Sprite_Tables.txt"
;#include "TestingBGData.txt"
;#include "TestingFGData.txt"


