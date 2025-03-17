using System;

namespace StbRectPackSharp;

internal static class StbRectPack
{
	public struct stbrp_context : IDisposable
	{
		public int width;

		public int height;

		public int align;

		public int init_mode;

		public int heuristic;

		public int num_nodes;

		public unsafe stbrp_node* active_head;

		public unsafe stbrp_node* free_head;

		public unsafe stbrp_node* extra;

		public unsafe stbrp_node* all_nodes;

		public unsafe stbrp_context(int nodesCount)
		{
			if (nodesCount <= 0)
			{
				throw new ArgumentOutOfRangeException("nodesCount");
			}
			width = (height = (align = (init_mode = (heuristic = (num_nodes = 0)))));
			active_head = (free_head = null);
			all_nodes = (stbrp_node*)CRuntime.malloc(sizeof(stbrp_node) * nodesCount);
			extra = (stbrp_node*)CRuntime.malloc(sizeof(stbrp_node) * 2);
		}

		public unsafe void Dispose()
		{
			if (all_nodes != null)
			{
				CRuntime.free(all_nodes);
				all_nodes = null;
			}
			if (extra != null)
			{
				CRuntime.free(extra);
				extra = null;
			}
		}
	}

	public struct stbrp_rect
	{
		public int id;

		public int w;

		public int h;

		public int x;

		public int y;

		public int was_packed;
	}

	public struct stbrp_node
	{
		public int x;

		public int y;

		public unsafe stbrp_node* next;
	}

	public struct stbrp__findresult
	{
		public int x;

		public int y;

		public unsafe stbrp_node** prev_link;
	}

	public const int STBRP_HEURISTIC_Skyline_default = 0;

	public const int STBRP_HEURISTIC_Skyline_BL_sortHeight = 0;

	public const int STBRP_HEURISTIC_Skyline_BF_sortHeight = 2;

	public const int STBRP__INIT_skyline = 1;

	public unsafe static void stbrp_setup_heuristic(stbrp_context* context, int heuristic)
	{
		if (context->init_mode == 1)
		{
			context->heuristic = heuristic;
			return;
		}
		throw new Exception("Mode " + context->init_mode + " is not supported.");
	}

	public unsafe static void stbrp_setup_allow_out_of_mem(stbrp_context* context, int allow_out_of_mem)
	{
		if (allow_out_of_mem != 0)
		{
			context->align = 1;
		}
		else
		{
			context->align = (context->width + context->num_nodes - 1) / context->num_nodes;
		}
	}

	public unsafe static void stbrp_init_target(stbrp_context* context, int width, int height, stbrp_node* nodes, int num_nodes)
	{
		int num = 0;
		for (num = 0; num < num_nodes - 1; num++)
		{
			nodes[num].next = nodes + (num + 1);
		}
		nodes[num].next = null;
		context->init_mode = 1;
		context->heuristic = 0;
		context->free_head = nodes;
		context->active_head = context->extra;
		context->width = width;
		context->height = height;
		context->num_nodes = num_nodes;
		stbrp_setup_allow_out_of_mem(context, 0);
		context->extra->x = 0;
		context->extra->y = 0;
		context->extra->next = context->extra + 1;
		context->extra[1].x = width;
		context->extra[1].y = 65535;
		context->extra[1].next = null;
	}

	public unsafe static int stbrp__skyline_find_min_y(stbrp_context* c, stbrp_node* first, int x0, int width, int* pwaste)
	{
		stbrp_node* ptr = first;
		int num = x0 + width;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		num2 = 0;
		num4 = 0;
		num3 = 0;
		while (ptr->x < num)
		{
			if (ptr->y > num2)
			{
				num4 += num3 * (ptr->y - num2);
				num2 = ptr->y;
				num3 = ((ptr->x >= x0) ? (num3 + (ptr->next->x - ptr->x)) : (num3 + (ptr->next->x - x0)));
			}
			else
			{
				int num5 = ptr->next->x - ptr->x;
				if (num5 + num3 > width)
				{
					num5 = width - num3;
				}
				num4 += num5 * (num2 - ptr->y);
				num3 += num5;
			}
			ptr = ptr->next;
		}
		*pwaste = num4;
		return num2;
	}

	public unsafe static stbrp__findresult stbrp__skyline_find_best_pos(stbrp_context* c, int width, int height)
	{
		int num = 1073741824;
		int num2 = 0;
		int num3 = 1073741824;
		stbrp__findresult result = default(stbrp__findresult);
		stbrp_node** ptr = null;
		width = width + c->align - 1;
		width -= width % c->align;
		if (width > c->width || height > c->height)
		{
			result.prev_link = null;
			result.x = (result.y = 0);
			return result;
		}
		stbrp_node* ptr2 = c->active_head;
		stbrp_node** ptr3 = &c->active_head;
		while (ptr2->x + width <= c->width)
		{
			int num4 = 0;
			int num5 = 0;
			num4 = stbrp__skyline_find_min_y(c, ptr2, ptr2->x, width, &num5);
			if (c->heuristic == 0)
			{
				if (num4 < num3)
				{
					num3 = num4;
					ptr = ptr3;
				}
			}
			else if (num4 + height <= c->height && (num4 < num3 || (num4 == num3 && num5 < num)))
			{
				num3 = num4;
				num = num5;
				ptr = ptr3;
			}
			ptr3 = &ptr2->next;
			ptr2 = ptr2->next;
		}
		num2 = ((ptr != null) ? (*ptr)->x : 0);
		if (c->heuristic == 2)
		{
			stbrp_node* ptr4 = c->active_head;
			ptr2 = c->active_head;
			ptr3 = &c->active_head;
			while (ptr4->x < width)
			{
				ptr4 = ptr4->next;
			}
			while (ptr4 != null)
			{
				int num6 = ptr4->x - width;
				int num7 = 0;
				int num8 = 0;
				while (ptr2->next->x <= num6)
				{
					ptr3 = &ptr2->next;
					ptr2 = ptr2->next;
				}
				num7 = stbrp__skyline_find_min_y(c, ptr2, num6, width, &num8);
				if (num7 + height <= c->height && num7 <= num3 && (num7 < num3 || num8 < num || (num8 == num && num6 < num2)))
				{
					num2 = num6;
					num3 = num7;
					num = num8;
					ptr = ptr3;
				}
				ptr4 = ptr4->next;
			}
		}
		result.prev_link = ptr;
		result.x = num2;
		result.y = num3;
		return result;
	}

	public unsafe static stbrp__findresult stbrp__skyline_pack_rectangle(stbrp_context* context, int width, int height)
	{
		stbrp__findresult result = stbrp__skyline_find_best_pos(context, width, height);
		if (result.prev_link == null || result.y + height > context->height || context->free_head == null)
		{
			result.prev_link = null;
			return result;
		}
		stbrp_node* free_head = context->free_head;
		free_head->x = result.x;
		free_head->y = result.y + height;
		context->free_head = free_head->next;
		stbrp_node* ptr = *result.prev_link;
		if (ptr->x < result.x)
		{
			stbrp_node* next = ptr->next;
			ptr->next = free_head;
			ptr = next;
		}
		else
		{
			*result.prev_link = free_head;
		}
		while (ptr->next != null && ptr->next->x <= result.x + width)
		{
			stbrp_node* next2 = ptr->next;
			ptr->next = context->free_head;
			context->free_head = ptr;
			ptr = next2;
		}
		free_head->next = ptr;
		if (ptr->x < result.x + width)
		{
			ptr->x = result.x + width;
		}
		return result;
	}

	public unsafe static int rect_height_compare(void* a, void* b)
	{
		if (((stbrp_rect*)a)->h > ((stbrp_rect*)b)->h)
		{
			return -1;
		}
		if (((stbrp_rect*)a)->h < ((stbrp_rect*)b)->h)
		{
			return 1;
		}
		if (((stbrp_rect*)a)->w <= ((stbrp_rect*)b)->w)
		{
			return (((stbrp_rect*)a)->w < ((stbrp_rect*)b)->w) ? 1 : 0;
		}
		return -1;
	}

	public unsafe static int rect_original_order(void* a, void* b)
	{
		if (((stbrp_rect*)a)->was_packed >= ((stbrp_rect*)b)->was_packed)
		{
			return (((stbrp_rect*)a)->was_packed > ((stbrp_rect*)b)->was_packed) ? 1 : 0;
		}
		return -1;
	}

	public unsafe static int stbrp_pack_rects(stbrp_context* context, stbrp_rect* rects, int num_rects)
	{
		int num = 0;
		int result = 1;
		for (num = 0; num < num_rects; num++)
		{
			rects[num].was_packed = num;
		}
		CRuntime.qsort(rects, (ulong)num_rects, (ulong)sizeof(stbrp_rect), rect_height_compare);
		for (num = 0; num < num_rects; num++)
		{
			if (rects[num].w == 0 || rects[num].h == 0)
			{
				rects[num].x = (rects[num].y = 0);
				continue;
			}
			stbrp__findresult stbrp__findresult = stbrp__skyline_pack_rectangle(context, rects[num].w, rects[num].h);
			if (stbrp__findresult.prev_link != null)
			{
				rects[num].x = stbrp__findresult.x;
				rects[num].y = stbrp__findresult.y;
			}
			else
			{
				rects[num].x = (rects[num].y = 65535);
			}
		}
		CRuntime.qsort(rects, (ulong)num_rects, (ulong)sizeof(stbrp_rect), rect_original_order);
		for (num = 0; num < num_rects; num++)
		{
			rects[num].was_packed = ((rects[num].x != 65535 || rects[num].y != 65535) ? 1 : 0);
			if (rects[num].was_packed == 0)
			{
				result = 0;
			}
		}
		return result;
	}
}
