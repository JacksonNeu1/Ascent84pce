
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




	;call sdcomp_set_fast
	;call sdcomp_set_flip
	;call sdcomp_reset_offset
	;ld hl, Tree_5 
	;ld de, Tree_5_Fast_F_0
	;call sdcomp_set_2bpc
	;call sprite_decompress
	
	;call sdcomp_set_fast
	;call sdcomp_reset_flip
	;call sdcomp_reset_offset
	;ld hl, Moss_0 
	;ld de, Moss_0_Fast_0
	;call sprite_decompress
	
	;call prgmpause
	;call prgmpause

	ld hl,0
	ld (cam_pos),hl
	ld (bg_cam_pos),hl
	
	
	ld a,%00000010;disable, 32768hz
	ld ($F20030),a	
	
	call setup_decompress_queue
	
	;Decompress sprites in preframes for setup
	ld hl,decompress_frame_up_pre2 
	call cfdc_direct_add_decompress_frame
	call continue_decompressions ;Run decompression (Will finish as timer has not started)
	ld hl,decompress_frame_up_pre1
	call cfdc_direct_add_decompress_frame
	call continue_decompressions ;Run decompression (Will finish as timer has not started)
	
	;call draw_fg
	
	ld a,0
	call cfdc_cam_move_up ;need to skip here for frame 0
	
	;call prgmpause

	call continue_decompressions
	
	;call prgmpause
	nop ;This needs to be here for some reason
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
	
	
	ld hl,$00E300
	ld (player_x_pos),hl 
	ld hl,$00EE00
	ld (player_y_pos),hl 
	

	;ld hl,BG_buffer+(160*5)
	;ld (dbgl_vram_line_start),hl

	;ld hl,BG_Data_1
	;ld a,10
	;call draw_bg_line
	
	
	;call prgmpause

	;call draw_mg

	call setup_bg ;after initial decompressions and cam setup
	;call prgmpause
	;call prgmpause
main_loop:

	

	;clear timer
	ld a,0
	ld ($F20000),a;32768hz
	ld ($F20001),a;128hz
	ld ($F20002),a;0.5hz
	ld ($F20003),a
	
	;Instructions here 
	
	
	;call input_cam_up
	jp get_inputs
get_inputs_return:
	;call move_bg Draw BG calls MoveBg
	;call player_move_debug
	call player_update
	call check_collisions
	
	call update_sine_vals
	ld a,(sine_8_7_2)
	call write_a_to_ram
	
	call update_animations
	
	
	call draw_bg

	
	
	;TEsting
	ld hl,0
	ld a,($F20000);32768hz
	ld l,a
	ld a,($F20001);128hz 
	ld h,a
	srl h ;div by 8
	rr l 
	srl h
	rr l 
	srl h
	rr l 
	ld a,l
	ld (draw_bg_time),a
	
	;TEST CLEAR BUFFER
	;ld hl,BG_buffer
	;ld de,(draw_buffer)
	;ld bc, 160*240
	;ldir
	
	call draw_mg
	
	call draw_mg2
	
	
	call player_draw 
	
	;TEsting
	ld hl,0
	ld a,($F20000);32768hz
	ld l,a
	ld a,($F20001);128hz 
	ld h,a
	srl h ;div by 8
	rr l 
	srl h
	rr l 
	srl h
	rr l 
	ld a,l
	ld (draw_mg_time),a
	
	call draw_fg
	;Leaves_4_Slow_1 has issue
	;Need to fix indexing of decompress segments
	
	;Sprite groups must all use the same load index of a given sprite, as they pull from same data
	
	
	
	;TEsting
	ld hl,0
	ld a,($F20000);32768hz
	ld l,a
	ld a,($F20001);128hz 
	ld h,a
	srl h ;div by 8
	rr l 
	srl h
	rr l 
	srl h
	rr l 
	ld a,l
	ld (draw_fg_time),a
	
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
	
	;Add to frame counter
	ld hl,(frameCount)
	inc hl
	ld (frameCount),hl
	

	
	;check if lcd has drawn first frame

	call check_for_decompress_calls
	;Sprite decompression will occur here
	call continue_decompressions
	
	;TEsting
	ld hl,0
	ld a,($F20000);32768hz
	ld l,a
	ld a,($F20001);128hz 
	ld h,a
	srl h ;div by 4
	rr l 
	srl h
	rr l 
	srl h
	rr l 
	ld a,l
	ld (decompress_time),a


	

	;Draw time bar at top of screen 
	ld hl, (draw_buffer)
	ld bc,0 
	ld a,(decompress_time)
	ld c,a 
	ld a, $55
	call _MemSet
	
	ld hl, (draw_buffer)
	ld bc,0 
	ld a,(draw_fg_time)
	ld c,a 
	ld a, $44
	call _MemSet
	
	ld hl, (draw_buffer)
	ld bc,0 
	ld a,(draw_mg_time)
	ld c,a 
	ld a, $33
	call _MemSet

	ld hl, (draw_buffer)
	ld bc,0 
	ld a,(draw_bg_time)
	ld c,a 
	ld a, $22
	call _MemSet
	
	
	
	ld hl, (draw_buffer)
	ld bc, 136  ; =1000/4 /2 for 2pix/bit 
	add hl,bc 
	ld a,$55
	ld (hl),a 
	
	
	;Again for thick line
	
	ld hl, (draw_buffer)
	ld bc,160
	add hl,bc 
	ld a,(decompress_time)
	ld c,a 
	ld a, $55
	call _MemSet
	
	ld hl, (draw_buffer)
	ld bc,160
	add hl,bc 
	ld a,(draw_fg_time)
	ld c,a 
	ld a, $44
	call _MemSet
	
	ld hl, (draw_buffer)
	ld bc,160
	add hl,bc
	ld a,(draw_mg_time)
	ld c,a 
	ld a, $33
	call _MemSet

	ld hl, (draw_buffer)
	ld bc,160
	add hl,bc
	ld a,(draw_bg_time)
	ld c,a 
	ld a, $22
	call _MemSet
	
	ld hl, (draw_buffer)
	ld bc,296
	add hl,bc 
	ld a,$55
	ld (hl),a 
	
	


	

	;swap draw buffers
	ld hl,(mpLcdBase)
	ld de,(draw_buffer)
	ld (mpLcdBase),de
	ld (draw_buffer),hl


	;wait until finished drawing second frame
	;Need to check clock here, there wont always be decompression to wait for
main_clock_check_loop:
	ld a,($F20001);128hz clock
	cp %00000011 ;check if reached 3 
	jp c,main_clock_check_loop ;msb <= 2, can continue  
	ld a,($F20000);32768hz clock
	cp %11101000;check if reached value 
	jp c,main_clock_check_loop ;msb = 3 and lsb < value, can continue
	


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
	ld hl, $d46000
	ld (hl),a 
	inc hl 
	ld (write_a_to_ram_addr),hl 
	pop hl 
	pop af 
	ret 
	
write_hl_to_ram:
	push hl 
	push de 
	ex de,hl
	ld hl,(write_a_to_ram_addr)
	ld (hl),de 
	inc hl
	inc hl
	inc hl
	ld (write_a_to_ram_addr),hl
	pop de 
	pop hl
	ret 

prgmpause: ;for testing, interrupts code until key pressed. will destroy af register
	push af
	push de 
	push hl 
	ex af,af';'
	push af 
	ei
	call _GetKey
	di
	pop af 
	ex af,af';'
	pop hl 
	pop de 
	pop af
	ret

cam_pos:;y position of lowest visible line in fg layer
	.dl 0
bg_cam_pos: ;y position of lowest visible line in bg layer (= cam pos / 8)
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

move_bg_time:
	.db 0
draw_bg_time:
	.db 0
draw_mg_time:
	.db 0
draw_fg_time:
	.db 0
decompress_time:
	.db 0


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

sd_test_a:
	.dl 0 






#include "timeTesting.txt"
#include "drawBGSprite.txt"
#include "drawFGSprite.txt"
#include "BetterSpriteDecompress.txt"
#include "drawFG.txt"
#include "SpriteDecompressManager.txt"
#include "getInputs.txt"
#include "PlayerController.txt"
#include "sineFunc.txt"
#include "animations.txt"
;#include "levelData.txt"
;#include "FGLevelData.txt"
;#include "testing/TestingCollisionData.txt"

#include "generated/BG_Data.txt"
#include "generated/MG_Data.txt"
#include "generated/MG2_Data.txt"
#include "generated/FG_Data.txt"
#include "generated/DecompressCalls.txt"
#include "generated/Palette_Setup.txt"
#include "generated/Sprite_Tables.txt"
#include "generated/Sprite_Data.txt"
#include "generated/SpriteEquates.txt"
#include "generated/Sprite_Groups.txt"
#include "generated/Collision_Data.txt"

;#include "testing/SpriteGroups.txt"
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


