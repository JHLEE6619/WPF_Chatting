   M S B A M L   `   `   `  ����C  ?Chatting, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:ChattingChatting  5   	       Chatting.Chat_room   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:Chatting     5   	     � Title$  	Chat_room��5   	   $	��600��6   $	��450��6(   .��5	      � ���	#FFFEF01B.+�5
   
   �� $
��Left=�6   $��	30,30,0,0q�6/   $
��Wrap=�6B   $��	대화방��5      $	��Top=�6   $��20(�60   $
��Bold"�6>   �� 5   
   -   5         ��Name$ LV_chat_record��5      $��	0,140,0,0q�6+   $	��Top=�6?   $��Center=�6W   $	��370��6t   $	��330��6�   ���	#FF9BBBD4��5      �� 5      .R�5      �� $��10q�6!    �� Orientation$ 
Horizontal=�6-   .+�5      �� $��30��6$   $	��100��60   $��14(�6T   $
��Bold"�6b   $
��Wrap=�6t   $��Center=�6�   $��Center=�6�   ��6<   �� *UserId+6�   �� 5      $��30��6$   $	��190��60   $��12(�6U   $
��Wrap=�6c   $
��Left=�6w   $��Center=�6�   ��6<   �� *	Content+6�   �� 5      $��30��6$   $��80��60   $��12(�6Q   $
��Wrap=�6_   $��Center=�6s   $��Center=�6�   ��6;   �� *Time+6�   5      5      5      �� 5   
   $��파일 전송��6   $��16(�6!   $
��Left=�6/   $��
40,100,0,0q�6J   $	��Top=�6^   $	��100��6v   �� 5   
   -   5         ��Name$ 
btn_invite��5      $��초대하기��6%   $��16(�64   $��Center=�6B   $��	0,100,0,0q�6_   $	��Top=�6r   $	��100��6�   6�   �� 5   
   -   5        $ btn_exit��5      $��퇴장��6#   $��16(�60   $
��Left=�6>   $��310,100,0,0q�6Y   $	��Top=�6n   $	��100��6�   6�   �� 5   
   -   5         ��Name$ 	Tbox_chat��5      $��16(�6%   $
��Left=�63   $��
40,500,0,0q�6N    �� TextWrapping$
 Wrap=�6b   $	��Top=�6v   $	��300��6�   
�������6�   �� 5   
   -   5        $ btn_send_chat��5      $��전송��6(   $��16(�65   $
��Left=�6C   $��360,499,0,0q�6^   $	��Top=�6s   $��50��6�   6�   5      5      