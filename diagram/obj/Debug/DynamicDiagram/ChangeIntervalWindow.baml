   M S B A M L   `   `   `  ����  diagram1    +diagram.DynamicDiagram.ChangeIntervalWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  更改时间间隔��5   	   $	��200��6   $	��300��6%   .��5      �  � RowDefinitions 5   
   �� 5      $��65 �6   �� 5      $��50 �6   �� 5	      $��50 �6   .+�5   
   �� $��刷新间隔(秒)��6   $��0��6"   $��15(�6/   $��Center=�6=   $��	20 0 20 0q�6X   �� 5   
   -   5        $��
tbInterval��5      $��0��6$   $��15(�61   $��30��6?   $	��150��6K   $��Right=�6W   $��0 0 20 0q�6s   �� 5   
   $��显示时间(分)��6   $��1��6"   $��15(�6/   $��Center=�6=   $��	20 0 20 0q�6X   �� 5   
   -   5        $��tbDisplayInterval��5      $��1��6+   $��15(�68   $��30��6F   $	��150��6R   $��Right=�6^   $��0 0 20 0q�6z   �� 5   
   -   5        $��btnReset��5      $��2��6!   $��重置��6.   $��80��6;   $��30��6F   $��Right=�6R   $��	0 0 110 0q�6n   6�   �� 5   
   -   5        $��
btnConfirm��5      $��2��6#   $��确定��60   $��80��6=   $��30��6H   $��Right=�6T   $��0 0 20 0q�6p   6�   5      5      